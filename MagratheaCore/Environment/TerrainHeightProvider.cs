using MagratheaCore.Utility;
using System;

namespace MagratheaCore.Environment
{
    public class TerrainHeightProvider : HeightProvider
    {
        #region Constants

        private const float HeightScale = 2000;

        #endregion

        #region Properties

        public NoiseProvider MainNoiseProvider { get; set; }

        public NoiseProvider ModulationNoiseProvider { get; set; }

        #endregion

        #region Constructors

        public TerrainHeightProvider(NoiseProvider mainNoiseProvider, NoiseProvider modulationNoiseProvider)
        {
            MainNoiseProvider = mainNoiseProvider;
            ModulationNoiseProvider = modulationNoiseProvider;
        }

        #endregion

        #region HeightProvider implementation

        public override float GetHeight(double x, double y, double z)
        {
            float mainHeight = MainNoiseProvider.GetValue(x, y, z);

            float modulationHeight = ModulationNoiseProvider.GetValue(x, y, z);
            modulationHeight = (float)Math.Pow(modulationHeight, 2);

            return mainHeight*modulationHeight*HeightScale;
        }

        #endregion
    }
}