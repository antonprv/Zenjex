// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.AssetManagement.Interfaces;
using UnityEngine;

namespace DevConsole.Backend.Infrastructure.Services.AssetManagement
{
  public class AssetLoader : IAssetLoader
  {
    public GameObject Load(string path) => Resources.Load<GameObject>(path);
    public T Load<T>(string path) where T : Object => Resources.Load<T>(path);
  }
}
