// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Infrastructure.Services.AssetManagement.Interfaces
{
  public interface IAssetLoader
  {
    public GameObject Load(string path);
    public T Load<T>(string path) where T : Object;
  }
}
