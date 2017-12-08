using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using System;


[InitializeOnLoad] public class EditorRunner {
	private static Runner runner;
	static EditorRunner () {
		Debug.Log("editor runner ready, is running:" + run);
		if (!run) {
			Debug.Log("let's run editor runner via menu. Unity > Window > VSIenum > Run.");
		}
		runner = new Runner();
	}

	public static bool run;

	[MenuItem("Window/VSIenum/Run")] public static void Run () {
		Debug.Log("start editor mainthread runner running.");
		run = true;
	}
	[MenuItem("Window/VSIenum/Stop")] public static void Stop () {
		Debug.Log("stop editor mainthread runner running.");
		run = false;

		// reload tests.
		runner.ReadyTests();
	}

	public class Runner {
		private Queue<IEnumerator> testEnums = new Queue<IEnumerator>();
		private IEnumerator runningTestEnum;
		
		

		public Runner () {
			/*
				このfuncをUnityEditorのupdateに渡し、testEnumsに含まれているIEnumを一件ずつ実行させる。
				一つのIEnumが終わったら、testEnumsから次のIEnumを取り出し実行するのを繰り返す。
			 */
			EditorApplication.CallbackFunction func = () => {
				if (!EditorRunner.run) {
					return;
				}

				if (runningTestEnum != null) {
					var result = runningTestEnum.MoveNext();
					
					if (result) {
						return;
					} else {
						// done running coroutine.
					}
				}

				// runningCoroutine is empty.
				if (testEnums.Any()) {
					runningTestEnum = testEnums.Dequeue();
				}
			};

			// run on Editor's main thred.(pseudo Player main thread.)
			EditorApplication.update += func;

			ReadyTests();
		}

		public void ReadyTests () {
			testEnums.Clear();

			// LoadAsset
			testEnums.Enqueue(
				LoadAsset()
			);
			
			// LoadAssetAsync
			testEnums.Enqueue(
				LoadAssetAsync()
			);

			// GetTexture
			testEnums.Enqueue(
				GetTexture()
			);

			// // CustomYieldConstruction これはなんか条件満たすと終わらないみたいなのをみてるんだけどまだ最小構成が不明
			// testEnums.Enqueue(
			// 	MyCustomYieldConstruction()
			// );
		}

		private IEnumerator LoadAsset () {
			Debug.Log("start LoadAsset.");
			
			Caching.CleanCache();
			
			var abRequest = UnityWebRequest.GetAssetBundle("https://github.com/sassembla/Autoya/raw/master/AssetBundles/Mac/1.0.0/bundlename");
			abRequest.Send();

			while (!abRequest.isDone) {
				yield return null;
			}

			if (abRequest.isError) {
				yield break;
			}

			Debug.Log("succeeded to download AssetBundle from web. start loading asset from AssetBundle.");
			var loadedAb = ((DownloadHandlerAssetBundle)abRequest.downloadHandler).assetBundle;
			
			using (var autoDispose = new AutoDispose(loadedAb)) {
				var asset = loadedAb.LoadAsset<Texture2D>("Assets/AutoyaTests/RuntimeData/AssetBundles/TestResources/textureName.png");

				Debug.Log("succeeded to load asset. asset:" + asset);
			}
		}

		private IEnumerator LoadAssetAsync () {
			Debug.Log("start LoadAssetAsync.");
			
			Caching.CleanCache();
			
			var abRequest = UnityWebRequest.GetAssetBundle("https://github.com/sassembla/Autoya/raw/master/AssetBundles/Mac/1.0.0/bundlename");
			abRequest.Send();

			while (!abRequest.isDone) {
				yield return null;
			}

			if (abRequest.isError) {
				yield break;
			}

			Debug.Log("succeeded to download AssetBundle from web. start loading asset from AssetBundle asynchronously.");
			var loadedAb = ((DownloadHandlerAssetBundle)abRequest.downloadHandler).assetBundle;
			
			using (var autoDispose = new AutoDispose(loadedAb)) {
				var asyncLoadAbRequest = loadedAb.LoadAssetAsync<Texture2D>("Assets/AutoyaTests/RuntimeData/AssetBundles/TestResources/textureName.png");

				// この部分がいつになっても終わらない。コンパイル処理が入るか、Play時だと動く。
				// Play時に動くのは当然として、コンパイル処理が入ると動くのはふしぎ。
				var timeLimit = DateTime.Now + TimeSpan.FromSeconds(10);

				while (!asyncLoadAbRequest.isDone) {
					if (timeLimit < DateTime.Now) {
						Debug.Log("abort LoadAssetAsync by timeout. 10 sec passed.");
						yield break;
					}

					yield return null;
				}

				if (asyncLoadAbRequest.asset) {
					Debug.Log("asset loaded.");
				}
			}
		}

		private IEnumerator GetTexture () {
			Debug.Log("start GetTexture. 5.6以上だとEditor updateでも動作するみたい。");

			var url = "https://www.google.co.jp/logos/doodles/2017/jan-ingenhouszs-287th-birthday-5733919419793408.3-s.png";
			using (var request = UnityWebRequest.GetTexture(url)) {
                var p = request.Send();

                while (!p.isDone) {
                    yield return null;
				}

				// done!
				Debug.Log("succeeded to get image or failed to get image.");
			}
			yield return null;
		}

        // public class MyCustomYieldConstructionClass : CustomYieldInstruction {
        //     public override bool keepWaiting {
		// 		get {
		// 			if (false) {
		// 				return false;
		// 			}
		// 			return true;
		// 		}
		// 	}
        // }




        public class AutoDispose : IDisposable {
			AssetBundle targetAb;
			public AutoDispose (AssetBundle targetAb) {
				this.targetAb = targetAb;
			}
            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (targetAb != null) {
							targetAb.Unload(true);
						}
                    }
                    disposedValue = true;
                }
            }


            // This code added to correctly implement the disposable pattern.
            void IDisposable.Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion

        }
    }
}
