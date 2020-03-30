using MagratheaCore.Environment.Enums;
using MagratheaCore.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MagratheaCore.Environment
{
    public class World : IDisposable
    {
        #region Fields

        private readonly GraphicsDevice _graphicsDevice;

        private readonly HeightProvider _heightProvider;

        private readonly QuadTreeNode[] _rootNodes;

        private readonly List<QuadTreeNode>[] _newRenderQueues, _newDisposalQueues;

        private Thread _rootNodeUpdateThread;

        #endregion

        #region Properties

        public DirectionLight Light { get; set; }

        public StarDome StarDome { get; private set; }

        public List<QuadTreeNode> RenderQueue { get; private set; }

        #endregion

        #region Constructors

        public World(GraphicsDevice graphicsDevice, HeightProvider heightProvider, float radius, DirectionLight light, Random starDomeRandom)
        {
            _graphicsDevice = graphicsDevice;

            _heightProvider = heightProvider;

            _rootNodes = new QuadTreeNode[6]
            {
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(radius, 0, 0), -Vector3.UnitZ, Vector3.UnitY, radius),
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(-radius, 0, 0), Vector3.UnitZ, Vector3.UnitY, radius),
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(0, radius, 0), Vector3.UnitZ, Vector3.UnitX, radius),
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(0, -radius, 0), -Vector3.UnitZ, Vector3.UnitX, radius),
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(0, 0, radius), -Vector3.UnitY, Vector3.UnitX, radius),
                new QuadTreeNode(_graphicsDevice, _heightProvider, NodeType.Root, null, 2*radius, new Vector3Double(0, 0, -radius), Vector3.UnitY, Vector3.UnitX, radius)
            };

            _newRenderQueues = new List<QuadTreeNode>[6]
            {
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>()
            };
            _newDisposalQueues = new List<QuadTreeNode>[6]
            {
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>(),
                new List<QuadTreeNode>()
            };

            Light = light;
            StarDome = new StarDome(starDomeRandom);
            RenderQueue = new List<QuadTreeNode>();
        }

        #endregion

        #region Private methods

        private void UpdateRootNodes(object parameters)
        {
            Vector3Double? cameraPosition = parameters as Vector3Double?;

            Parallel.For(0, 6, i =>
            {
                _newRenderQueues[i].Clear();
                _newDisposalQueues[i].Clear();

                _rootNodes[i].UpdateChildrenRecursively(_graphicsDevice, _heightProvider, cameraPosition.Value, _newRenderQueues[i], _newDisposalQueues[i], 0);
                _rootNodes[i].UpdateNeighboursRecursively();
            });
        }

        #endregion

        #region Methods

        public void Update(Vector3Double cameraPosition)
        {
            // Absorb queues and update root nodes
            if (_rootNodeUpdateThread == null || !_rootNodeUpdateThread.IsAlive)
            {
                RenderQueue.Clear();
                foreach (List<QuadTreeNode> newRenderQueue in _newRenderQueues)
                {
                    RenderQueue.AddRange(newRenderQueue);
                }
                RenderQueue.ForEach(node => node.FixCracks());

                foreach (List<QuadTreeNode> newDisposalQueue in _newDisposalQueues)
                {
                    newDisposalQueue.ForEach(node => node.Dispose());
                }

                _rootNodeUpdateThread = new Thread(UpdateRootNodes);
                _rootNodeUpdateThread.Start(cameraPosition);
            }
        }

        #endregion

        #region IDisposable overrides

        protected virtual void Dispose(bool disposing)
        {
            if (_rootNodeUpdateThread != null)
            {
                _rootNodeUpdateThread.Join();
            }

            foreach (List<QuadTreeNode> newDisposalQueue in _newDisposalQueues)
            {
                newDisposalQueue.ForEach(node => node.Dispose());
            }

            foreach (QuadTreeNode rootNode in _rootNodes)
            {
                rootNode.Dispose();
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
