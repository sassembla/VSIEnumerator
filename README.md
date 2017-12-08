# これはなに？

* 特定のメソッドがUnityEditor update経由だと動作しないことがある
* だいたいIEnumerator関連
* それらの動作例をまとめたもの

以前は動作しなかったんだけど新しいverのUnityだと動作してくれて嬉しい、というものがあり、それら動くケースも入っている。


## LoadAsset/LoadAssetAsync

- LoadAssetは動作するが、LoadAssetAsyncは動作しない(完了しない)
- 例えばLoadAssetAsync中にコードのコンパイルが発生したりすると動作する。

## UnityWebRequest.GetTexture(string url)
- 以前のUnityだと動作しなかった(完了しない)のだけれど、今回試したUnity 5.6.1p4 以降では完了する模様。おさわがせしました。

and more(まだ最小構成にできてないやつがあって、適当に追記予定)