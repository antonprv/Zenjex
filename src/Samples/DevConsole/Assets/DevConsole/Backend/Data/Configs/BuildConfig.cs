// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Data.Configs.Types;

namespace DevConsole.Backend.Data.Configs
{
  [UnityEngine.CreateAssetMenu(fileName = "BuildConfig", menuName = "StaticData/Config/BuildConfig")]
  public class GameBuildData : UnityEngine.ScriptableObject
  {
    [NoNone]
    public BuildConfiguration BuildConfiguration = BuildConfiguration.Development;
  }
}
