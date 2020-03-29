using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagratheaCore.Utility;
using MagratheaCore.Environment.Enums;

namespace MagratheaCore.Environment
{
    public class QuadTreeNode : IDisposable
    {
        #region Constants

        /// <summary>
        /// Must be odd number and > 1
        /// </summary>
        private const int VerticesPerEdge = 15;

        private const float EdgeLengthLimit = 10;
        private const float SplitDistanceMultiplier = 4;

        private const int MaxChildCreationDepth = 2;

        #endregion

        #region Fields

        private static Dictionary<IndexBufferSelection, IndexBuffer> _indexBuffers;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly HeightProvider _heightProvider;

        private readonly NodeType _nodeType;

        private readonly QuadTreeNode _parentNode;

        private readonly float _edgeLengthFlat, _halfEdgeLengthFlat, _quarterEdgeLengthFlat;
        private readonly Vector3Double _originPositionFlat;
        private readonly Vector3 _upDirectionFlat, _rightDirectionFlat;

        private readonly float _radiusSphere;
        
        private bool _hasChildren;

        #endregion

        #region Properties

        #region Child nodes

        public QuadTreeNode ChildUpLeft { get; private set; }

        public QuadTreeNode ChildUpRight { get; private set; }

        public QuadTreeNode ChildDownRight { get; private set; }

        public QuadTreeNode ChildDownLeft { get; private set; }

        #endregion

        #region Neighbour nodes

        public QuadTreeNode NeighbourUp { get; private set; }

        public QuadTreeNode NeighbourRight { get; private set; }

        public QuadTreeNode NeighbourDown { get; private set; }

        public QuadTreeNode NeighbourLeft { get; private set; }

        #endregion

        public Vector3Double OriginPositionSphere { get; private set; }
        
        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public BoundingBox BoundingBox { get; private set; }

        #endregion

        #region Constructors

        public QuadTreeNode(GraphicsDevice graphicsDevice, HeightProvider heightProvider, NodeType nodeType, QuadTreeNode parentNode, float edgeLengthFlat, Vector3Double originPositionFlat, Vector3 upDirectionFlat, Vector3 rightDirectionFlat, float radiusSphere)
        {
            _graphicsDevice = graphicsDevice;

            _heightProvider = heightProvider;

            _nodeType = nodeType;

            _parentNode = parentNode;

            _edgeLengthFlat = edgeLengthFlat;
            _halfEdgeLengthFlat = _edgeLengthFlat/2;
            _quarterEdgeLengthFlat = _edgeLengthFlat/4;
            _originPositionFlat = originPositionFlat;
            _upDirectionFlat = upDirectionFlat;
            _rightDirectionFlat = rightDirectionFlat;

            _radiusSphere = radiusSphere;

            Vector3 originPositionSphereNormal = Vector3Double.Normalize(_originPositionFlat);
            Vector3Double originPositionSpherePosition = new Vector3Double(_radiusSphere*originPositionSphereNormal);
            OriginPositionSphere = originPositionSpherePosition + _heightProvider.GetHeight(originPositionSpherePosition)*originPositionSphereNormal;

            IndexBuffer = _indexBuffers[IndexBufferSelection.Base];
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Resulting triangles are wound CCW and filled from down-left to up-right
        /// </summary>
        private static IndexBuffer CalculateIndexBuffer(GraphicsDevice graphicsDevice, bool upCrackFix, bool rightCrackFix, bool downCrackFix, bool leftCrackFix)
        {
            List<ushort> resultIndices = new List<ushort>();
            for (int y = 0; y < VerticesPerEdge - 1; y++)
            {
                bool slantRight = y % 2 == 0;

                for (int x = 0; x < VerticesPerEdge - 1; x++)
                {
                    ushort dlIndex = (ushort)(x + y*VerticesPerEdge);
					ushort drIndex = (ushort)(dlIndex + 1);
					ushort ulIndex = (ushort)(x + (y + 1)*VerticesPerEdge);
					ushort urIndex = (ushort)(ulIndex + 1);

					ushort[] triangle1 = slantRight ? new ushort[3] { ulIndex, dlIndex, urIndex } : new ushort[3] { ulIndex, dlIndex, drIndex };
                    ushort[] triangle2 = slantRight ?  new ushort[3] { dlIndex, drIndex, urIndex } : new ushort[3] { ulIndex, drIndex, urIndex };

                    // Perform requested crack fixing
                    if (upCrackFix && y == VerticesPerEdge - 2)
                    {
                        if (x % 2 == 0)
                        {
                            triangle2 = new ushort[3] { ulIndex, drIndex, (ushort)(urIndex + 1) };
                        }
                        else
                        {
                            triangle1 = null;
                        }
                    }
                    if (rightCrackFix && x == VerticesPerEdge - 2)
                    {
                        if (y % 2 == 0)
                        {
                            triangle2 = new ushort[3] { (ushort)(urIndex + VerticesPerEdge), ulIndex, drIndex };
                        }
                        else
                        {
                            triangle2 = null;
                        }
                    }
                    if (downCrackFix && y == 0)
                    {
                        if (x % 2 == 0)
                        {
                            triangle2 = new ushort[3] { (ushort)(drIndex + 1), urIndex, dlIndex };
                        }
                        else
                        {
                            triangle1 = null;
                        }
                    }
                    if (leftCrackFix && x == 0)
                    {
                        if (y % 2 == 0)
                        {
                            triangle1 = new ushort[3] { dlIndex, urIndex, (ushort)(ulIndex + VerticesPerEdge) };
                        }
                        else
                        {
                            triangle1 = null;
                        }
                    }

                    if (triangle1 != null)
                    {
                        resultIndices.AddRange(triangle1);
                    }
                    if (triangle2 != null)
                    {
                        resultIndices.AddRange(triangle2);
                    }

                    slantRight = !slantRight;
                }
            }

            ushort[] resultIndicesData = resultIndices.ToArray();

            IndexBuffer result = new IndexBuffer(graphicsDevice, typeof(ushort), resultIndicesData.Length, BufferUsage.WriteOnly);
            result.SetData(resultIndicesData);

            return result;
        }

        private void CalculateContents()
        {
            float vertexSpacingFlat = _edgeLengthFlat/(VerticesPerEdge - 1);

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            List<TerrainVertex> resultVertices = new List<TerrainVertex>();
            for (int y = 0; y < VerticesPerEdge; y++)
            {
                for (int x = 0; x < VerticesPerEdge; x++)
                {
                    Vector3Double positionWorldFlat = _originPositionFlat + (x*vertexSpacingFlat - _halfEdgeLengthFlat)*_rightDirectionFlat + (y*vertexSpacingFlat - _halfEdgeLengthFlat)*_upDirectionFlat;

                    Vector3 positionWorldSphereNormal = Vector3Double.Normalize(positionWorldFlat);
                    Vector3Double positionWorldSpherePosition = new Vector3Double(_radiusSphere*positionWorldSphereNormal);
                    Vector3Double positionWorldSphere = positionWorldSpherePosition + _heightProvider.GetHeight(positionWorldSpherePosition)*positionWorldSphereNormal;

                    Vector3 position = (positionWorldSphere - OriginPositionSphere).ToVector3();
                    Vector3 normal = _heightProvider.GetNormalFromFiniteOffset(positionWorldSpherePosition);

                    TerrainVertex vertex = new TerrainVertex(position, normal);
                    resultVertices.Add(vertex);

                    min = Vector3.Min(min, vertex.Position);
                    max = Vector3.Max(max, vertex.Position);
                }
            }

            TerrainVertex[] resultVerticesData = resultVertices.ToArray();

            VertexBuffer = new VertexBuffer(_graphicsDevice, TerrainVertex.VertexDeclaration, resultVerticesData.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(resultVerticesData);

            BoundingBox = new BoundingBox(min, max);
        }

        #endregion

        #region Methods

        public static void CalculateIndexBuffers(GraphicsDevice graphicsDevice)
        {
            _indexBuffers = new Dictionary<IndexBufferSelection, IndexBuffer>
            {
                { IndexBufferSelection.Base, CalculateIndexBuffer(graphicsDevice, false, false, false, false) },
                { IndexBufferSelection.UCrackFix, CalculateIndexBuffer(graphicsDevice, true, false, false, false) },
                { IndexBufferSelection.RCrackFix, CalculateIndexBuffer(graphicsDevice, false, true, false, false) },
                { IndexBufferSelection.DCrackFix, CalculateIndexBuffer(graphicsDevice, false, false, true, false) },
                { IndexBufferSelection.LCrackFix, CalculateIndexBuffer(graphicsDevice, false, false, false, true) },
                { IndexBufferSelection.ULCrackFix, CalculateIndexBuffer(graphicsDevice, true, false, false, true) },
                { IndexBufferSelection.URCrackFix, CalculateIndexBuffer(graphicsDevice, true, true, false, false) },
                { IndexBufferSelection.DRCrackFix, CalculateIndexBuffer(graphicsDevice, false, true, true, false) },
                { IndexBufferSelection.DLCrackFix, CalculateIndexBuffer(graphicsDevice, false, false, true, true) }
            };
        }

        public void UpdateChildrenRecursively(GraphicsDevice graphicsDevice, HeightProvider heightProvider, Vector3Double cameraPosition, List<QuadTreeNode> renderQueue, List<QuadTreeNode> disposalQueue, int childCreationDepth)
        {
            // Node should have children
            if (childCreationDepth < MaxChildCreationDepth && _halfEdgeLengthFlat > EdgeLengthLimit && Vector3Double.Distance(OriginPositionSphere, cameraPosition) < SplitDistanceMultiplier*_edgeLengthFlat)
            {
                // Add children if they aren't there
                if (!_hasChildren)
                {
                    ChildUpLeft = new QuadTreeNode(graphicsDevice, heightProvider, NodeType.ChildUpLeft, this, _halfEdgeLengthFlat, _originPositionFlat + _quarterEdgeLengthFlat*(_upDirectionFlat - _rightDirectionFlat), _upDirectionFlat, _rightDirectionFlat, _radiusSphere);
                    ChildUpRight = new QuadTreeNode(graphicsDevice, heightProvider, NodeType.ChildUpRight, this, _halfEdgeLengthFlat, _originPositionFlat + _quarterEdgeLengthFlat*(_upDirectionFlat + _rightDirectionFlat), _upDirectionFlat, _rightDirectionFlat, _radiusSphere);
                    ChildDownRight = new QuadTreeNode(graphicsDevice, heightProvider, NodeType.ChildDownRight, this, _halfEdgeLengthFlat, _originPositionFlat + _quarterEdgeLengthFlat*(-_upDirectionFlat + _rightDirectionFlat), _upDirectionFlat, _rightDirectionFlat, _radiusSphere);
                    ChildDownLeft = new QuadTreeNode(graphicsDevice, heightProvider, NodeType.ChildDownLeft, this, _halfEdgeLengthFlat, _originPositionFlat + _quarterEdgeLengthFlat*(-_upDirectionFlat - _rightDirectionFlat), _upDirectionFlat, _rightDirectionFlat, _radiusSphere);

                    _hasChildren = true;

                    childCreationDepth += 1;
                }

                ChildUpLeft.UpdateChildrenRecursively(graphicsDevice, heightProvider, cameraPosition, renderQueue, disposalQueue, childCreationDepth);
                ChildUpRight.UpdateChildrenRecursively(graphicsDevice, heightProvider, cameraPosition, renderQueue, disposalQueue, childCreationDepth);
                ChildDownRight.UpdateChildrenRecursively(graphicsDevice, heightProvider, cameraPosition, renderQueue, disposalQueue, childCreationDepth);
                ChildDownLeft.UpdateChildrenRecursively(graphicsDevice, heightProvider, cameraPosition, renderQueue, disposalQueue, childCreationDepth);
            }
            // Node shouldn't have children
            else
            {
                // Remove children and mark them for disposal if they are there
                if (_hasChildren)
                {
                    disposalQueue.Add(ChildUpLeft);
                    disposalQueue.Add(ChildUpRight);
                    disposalQueue.Add(ChildDownRight);
                    disposalQueue.Add(ChildDownLeft);

                    ChildUpLeft = null;
                    ChildUpRight = null;
                    ChildDownRight = null;
                    ChildDownLeft = null;

                    _hasChildren = false;
                }
                
                if (VertexBuffer == null)
                {
                    CalculateContents();
                }

                renderQueue.Add(this);
            }
        }

        public void UpdateNeighboursRecursively()
        {
            switch (_nodeType)
            {
                case NodeType.ChildUpLeft:
                    if (_parentNode.NeighbourUp != null)
                    {
                        NeighbourUp = _parentNode.NeighbourUp.ChildDownLeft;
                    }
                    NeighbourRight = _parentNode.ChildUpRight;
                    NeighbourDown = _parentNode.ChildDownLeft;
                    if (_parentNode.NeighbourLeft != null)
                    {
                        NeighbourLeft = _parentNode.NeighbourLeft.ChildUpRight;
                    }
                    break;

                case NodeType.ChildUpRight:
                    if (_parentNode.NeighbourUp != null)
                    {
                        NeighbourUp = _parentNode.NeighbourUp.ChildDownRight;
                    }
                    if (_parentNode.NeighbourRight != null)
                    {
                        NeighbourRight = _parentNode.NeighbourRight.ChildUpLeft;
                    }
                    NeighbourDown = _parentNode.ChildDownRight;
                    NeighbourLeft = _parentNode.ChildUpLeft;
                    break;

                case NodeType.ChildDownRight:
                    NeighbourUp = _parentNode.ChildUpRight;
                    if (_parentNode.NeighbourRight != null)
                    {
                        NeighbourRight = _parentNode.NeighbourRight.ChildDownLeft;
                    }
                    if (_parentNode.NeighbourDown != null)
                    {
                        NeighbourDown = _parentNode.NeighbourDown.ChildUpRight;
                    }
                    NeighbourLeft = _parentNode.ChildDownLeft;
                    break;

                case NodeType.ChildDownLeft:
                    NeighbourUp = _parentNode.ChildUpLeft;
                    NeighbourRight = _parentNode.ChildDownRight;
                    if (_parentNode.NeighbourDown != null)
                    {
                        NeighbourDown = _parentNode.NeighbourDown.ChildUpLeft;
                    }
                    if (_parentNode.NeighbourLeft != null)
                    {
                        NeighbourLeft = _parentNode.NeighbourLeft.ChildDownRight;
                    }
                    break;
            }

            if (_hasChildren)
            {
                ChildUpLeft.UpdateNeighboursRecursively();
                ChildUpRight.UpdateNeighboursRecursively();
                ChildDownLeft.UpdateNeighboursRecursively();
                ChildDownRight.UpdateNeighboursRecursively();
            }
        }

        public void FixCracks()
        {
            IndexBuffer = _indexBuffers[IndexBufferSelection.Base];

            switch (_nodeType)
            {
                case NodeType.ChildUpLeft:
                    if (NeighbourUp == null && NeighbourLeft == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.ULCrackFix];
                    }
                    else if (NeighbourUp == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.UCrackFix];
                    }
                    else if (NeighbourLeft == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.LCrackFix];
                    }
                    break;

                case NodeType.ChildUpRight:
                    if (NeighbourUp == null && NeighbourRight == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.URCrackFix];
                    }
                    else if (NeighbourUp == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.UCrackFix];
                    }
                    else if (NeighbourRight == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.RCrackFix];
                    }
                    break;

                case NodeType.ChildDownRight:
                    if (NeighbourDown == null && NeighbourRight == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.DRCrackFix];
                    }
                    else if (NeighbourDown == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.DCrackFix];
                    }
                    else if (NeighbourRight == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.RCrackFix];
                    }
                    break;

                case NodeType.ChildDownLeft:
                    if (NeighbourDown == null && NeighbourLeft == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.DLCrackFix];
                    }
                    else if (NeighbourDown == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.DCrackFix];
                    }
                    else if (NeighbourLeft == null)
                    {
                        IndexBuffer = _indexBuffers[IndexBufferSelection.LCrackFix];
                    }
                    break;
            }
        }

        #endregion

        #region IDisposable overrides

        protected virtual void Dispose(bool disposing)
        {
            if (_hasChildren)
            {
                ChildUpLeft.Dispose();
                ChildUpRight.Dispose();
                ChildDownRight.Dispose();
                ChildDownLeft.Dispose();
            }

            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
