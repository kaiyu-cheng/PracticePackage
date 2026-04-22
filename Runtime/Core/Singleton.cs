#if PRACTICE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Practice.Core
{
	public abstract class Singleton<T_TYPE> : MonoBehaviour where T_TYPE : MonoBehaviour
	{
		// Properties
		public static T_TYPE Instance { get{ return GetInstance(); } }

		// Protected
		protected virtual void Awake()
		{
			_Instance = this as T_TYPE;
		}
#if PRACTICE_UNITASK
		protected virtual UniTask Initialize()
		{
			return UniTask.CompletedTask;
		}
#endif
        // Private
        private static T_TYPE GetInstance()
		{
			if (_Instance == null)
			{
				_Instance = FindAnyObjectByType<T_TYPE>();

				if (_Instance == null)
				{
					Debug.LogError($"[PRACTICE.Framework] {typeof(T_TYPE).Name} No instance exist");
				}
			}
			return _Instance;
		}

		// Variable
		private static T_TYPE _Instance;
	}
}