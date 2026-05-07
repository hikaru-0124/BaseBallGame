# BaseBallGame

Unityで作る3D野球ゲームの最小プロトタイプです。  
このリポジトリには次の2つが入っています。

- **ブラウザ版**（すぐ遊べる）
- **Unity版**（C#スクリプト）

## ブラウザ版（すぐ遊ぶ）

1. `index.html` をブラウザで開く  
2. `Space` でスイング  
3. 画面上に `HIT / OUT / STRIKE` が表示されます

### 操作

- `Space`: スイング
- `R`: スコアリセット

> 補足: ローカルファイル制限がある環境では、`python3 -m http.server` で起動して `http://localhost:8000` から開いてください。

## 1. Unityプロジェクト作成

1. Unity Hubで **3D (URP でも Built-in でも可)** の新規プロジェクトを作る  
2. このリポジトリの `Assets/Scripts` 配下の `.cs` を、作成したUnityプロジェクトの `Assets/Scripts` にコピー

## 2. シーン構成

1. `Plane` を追加して地面にする（位置 `0,0,0`）
2. `Cube` を追加してバットにする（名前 `Bat`）
3. `Bat` に `BoxCollider` をつけ、`Is Trigger` をON
4. `Bat` に `HitDetector` をアタッチ
5. `BatPivot`（空オブジェクト）を作成し、`Bat` を子にする
6. `Batter`（Capsule）を原点付近に置く（例: `0,1,0`）
7. `Batter` に `BatterController` をアタッチし、`Bat Pivot` に `BatPivot` を割り当て
8. `PitchOrigin`（空オブジェクト）を投手位置に置く（例: `0,1.4,13`）
9. `StrikeZoneCenter`（空オブジェクト）を打者前に置く（例: `0,1.1,0.7`）
10. `GameManager`（空オブジェクト）を作成し、`GameManager` スクリプトをアタッチ
11. `Pitcher`（空オブジェクト）を作成し、`BallPitcher` スクリプトをアタッチ
12. `GameManager` の参照欄に `Pitcher` と `BatterController` と `HitDetector` を割り当て
13. `BallPitcher` の `Pitch Origin` と `Strike Zone Center` を割り当て

## 3. ボールPrefab作成

1. `Sphere` を作成して名前を `Baseball`
2. `Rigidbody` を追加（Use Gravity ON）
3. `Collider` はそのまま（SphereCollider）
4. `Baseball` スクリプトをアタッチ
5. タグを `Ball` に設定（存在しなければ作成）
6. `Project` ウィンドウへドラッグして Prefab 化
7. シーン上の `Baseball` は削除
8. `BallPitcher` の `Ball Prefab` に作った Prefab を割り当て

## 4. 操作

- `Space`: スイング
- 画面左上に現在の結果とカウントが表示されます

## 5. 調整ポイント

- `BallPitcher.pitchSpeed`: 球速
- `BallPitcher.spreadX / spreadY`: 配球のバラつき
- `BatterController.swingDuration / hitWindowStart / hitWindowEnd`: スイングタイミング
- `HitDetector.minHitSpeed / maxHitSpeed`: 打球速度
- `GameManager.hitDistanceThreshold`: ヒット判定の距離
