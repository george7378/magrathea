using MagratheaCore.Environment;
using MagratheaCore.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MagratheaCore
{
	public class MagratheaGame : Game
	{
		#region Fields

		private readonly GraphicsDeviceManager _graphics;

		private MouseState _oldMouseState;
		private KeyboardState _oldKeyboardState;

		private World _world;
		private CentredCamera _camera;

		private Effect _terrainEffect;

		private Texture2D _sunTexture;
		private Texture2D _starTexture;

		private SpriteBatch _spriteBatch;

		private bool _mouseLookActive;

		#endregion

		#region Constructors

		public MagratheaGame()
		{
			_graphics = new GraphicsDeviceManager(this) { PreferMultiSampling = true };

			Content.RootDirectory = "Content";
		}

		#endregion

		#region Private methods

		#region Content drawing

		private void DrawTerrain()
		{
			_terrainEffect.CurrentTechnique = _terrainEffect.Techniques["TerrainTechnique"];

			_terrainEffect.Parameters["LightPower"].SetValue(_world.Light.Power);
			_terrainEffect.Parameters["AmbientLightPower"].SetValue(_world.Light.AmbientPower);
			_terrainEffect.Parameters["LightDirection"].SetValue(_world.Light.Direction);

			foreach (QuadTreeNode node in _world.RenderQueue)
			{
				Matrix nodeWorldMatrix = _camera.GetWorldMatrix(node.OriginPositionSphere);

				if (_camera.Frustum.Intersects(new BoundingBox(Vector3.Transform(node.BoundingBox.Min, nodeWorldMatrix), Vector3.Transform(node.BoundingBox.Max, nodeWorldMatrix))))
				{
					_terrainEffect.Parameters["World"].SetValue(nodeWorldMatrix);
					_terrainEffect.Parameters["WorldViewProjection"].SetValue(nodeWorldMatrix*_camera.ViewMatrix*_camera.ProjectionMatrix);

					foreach (EffectPass pass in _terrainEffect.CurrentTechnique.Passes)
					{
						pass.Apply();

						GraphicsDevice.SetVertexBuffer(node.VertexBuffer);
						GraphicsDevice.Indices = node.IndexBuffer;

						GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, node.IndexBuffer.IndexCount/3);

						GraphicsDevice.SetVertexBuffer(null);
						GraphicsDevice.Indices = null;
					}
				}
			}
		}

		private void DrawSky()
		{
			_spriteBatch.Begin();

			// Stars
			foreach (Star star in _world.StarDome.Stars)
			{
				if (Vector3.Dot(_camera.Orientation.Forward, star.DomePosition) > 0)
				{
					Vector3 starScreenPosition = GraphicsDevice.Viewport.Project(star.DomePosition, _camera.ProjectionMatrix, _camera.ViewMatrix, Matrix.Identity);

					_spriteBatch.Draw(_starTexture, new Vector2((int)starScreenPosition.X, (int)starScreenPosition.Y), null, star.Colour, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				}
			}

			// Sun
			if (Vector3.Dot(_camera.Orientation.Forward, -_world.Light.Direction) > 0)
			{
				Vector3 sunScreenPosition = GraphicsDevice.Viewport.Project(-_world.Light.Direction, _camera.ProjectionMatrix, _camera.ViewMatrix, Matrix.Identity);

				_spriteBatch.Draw(_sunTexture, new Vector2(sunScreenPosition.X, sunScreenPosition.Y), null, Color.White, 0, new Vector2(128, 128), 1, SpriteEffects.None, 0);
			}

			_spriteBatch.End();

			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		#endregion

		#region Misc.

		private void ProcessInput()
		{
			MouseState newMouseState = Mouse.GetState();
			KeyboardState newKeyboardState = Keyboard.GetState();

			// Various states
			if (_oldKeyboardState.IsKeyDown(Keys.C) && newKeyboardState.IsKeyUp(Keys.C))
			{
				_mouseLookActive = !_mouseLookActive;
			}

			if (_oldKeyboardState.IsKeyDown(Keys.Up) && newKeyboardState.IsKeyUp(Keys.Up))
			{
				_camera.MovementSpeed = _camera.MovementSpeed < 100000 ? _camera.MovementSpeed*10 : _camera.MovementSpeed;
			}

			if (_oldKeyboardState.IsKeyDown(Keys.Down) && newKeyboardState.IsKeyUp(Keys.Down))
			{
				_camera.MovementSpeed = _camera.MovementSpeed > 10 ? _camera.MovementSpeed/10 : _camera.MovementSpeed;
			}

			// Camera movement
			Vector2 strafeVector = new Vector2(newKeyboardState.IsKeyDown(Keys.A) ? -1 : newKeyboardState.IsKeyDown(Keys.D) ? 1 : 0, newKeyboardState.IsKeyDown(Keys.S) ? -1 : newKeyboardState.IsKeyDown(Keys.W) ? 1 : 0);
			if (strafeVector.Length() > 0)
			{
				_camera.Position += _camera.MovementSpeed*(_camera.Orientation.Forward*strafeVector.Y + _camera.Orientation.Right*strafeVector.X);
			}

			Matrix yawDelta = Matrix.Identity;
			Matrix pitchDelta = Matrix.Identity;
			if (_mouseLookActive)
			{
				yawDelta = Matrix.CreateFromAxisAngle(_camera.Orientation.Up, -0.01f*(newMouseState.X - _oldMouseState.X));
				pitchDelta = Matrix.CreateFromAxisAngle(_camera.Orientation.Right, -0.01f*(newMouseState.Y - _oldMouseState.Y));

				Mouse.SetPosition((int)(GraphicsDevice.Viewport.Width/2.0f), (int)(GraphicsDevice.Viewport.Height/2.0f));
				newMouseState = Mouse.GetState();
			}

			Matrix rollDelta = Matrix.CreateFromAxisAngle(_camera.Orientation.Forward, 0.01f*(newKeyboardState.IsKeyDown(Keys.Q) ? -1 : newKeyboardState.IsKeyDown(Keys.E) ? 1 : 0));

			_camera.Orientation = Globals.OrthonormaliseMatrix(_camera.Orientation*yawDelta*pitchDelta*rollDelta);

			_oldMouseState = newMouseState;
			_oldKeyboardState = newKeyboardState;
		}

		#endregion

		#endregion

		#region Game overrides

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			QuadTreeNode.CalculateIndexData(GraphicsDevice);

			_oldMouseState = Mouse.GetState();
			_oldKeyboardState = Keyboard.GetState();

			int worldSeed = 1;
			NoiseProvider mainNoiseProvider = new NoiseProvider(worldSeed, 4, 0.3f, 6000);
			NoiseProvider modulationNoiseProvider = new NoiseProvider(worldSeed + 1, 2, 0.2f, 50000);
			DirectionLight light = new DirectionLight(Vector3.Normalize(new Vector3(0, -0.3f, 1)), 1, 0.1f);
			_world = new World(GraphicsDevice, new TerrainHeightProvider(mainNoiseProvider, modulationNoiseProvider), 1737000, light, new Random(worldSeed));

			Matrix cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.Viewport.AspectRatio, CentredCamera.NearPlaneDistance, CentredCamera.FarPlaneDistance);
			_camera = new CentredCamera()
			{
				Position = new Vector3Double(0, 1740000, 0),
				Orientation = Matrix.Identity,
				ProjectionMatrix = cameraProjectionMatrix,
				MovementSpeed = 1000
			};

			_starTexture = new Texture2D(GraphicsDevice, 1, 1);
			_starTexture.SetData(new Color[] { Color.White });

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			_terrainEffect = Content.Load<Effect>("Effects/TerrainEffect");

			_sunTexture = Content.Load<Texture2D>("Textures/sun");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			ProcessInput();

			_world.Update(_camera.Position);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// PASS 1: Draw the scene
			GraphicsDevice.Clear(Color.Black);

				DrawSky();
				DrawTerrain();

			base.Draw(gameTime);
		}

		#endregion
	}
}
