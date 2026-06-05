# WebGL/HTML Build Guide

このプロジェクトは、1080 x 1920 のスマホ縦画面を基準にしたUnity WebGLゲームとして公開する前提です。

## Unity設定

推奨設定は以下です。

- Platform: `WebGL`
- Scenes In Build:
  - `Assets/Scenes/TitleScene.unity`
  - `Assets/Scenes/GameScene.unity`
  - `Assets/Scenes/ResultScene.unity`
- Player Settings:
  - Resolution: `1080 x 1920`
  - Default Orientation: `Portrait`
  - WebGL Template: `PROJECT:FixedAspect`
  - Decompression Fallback: `On`
- Canvas:
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: `1080 x 1920`
  - Match Width Or Height: `0.5`
  - Render Mode: `Screen Space - Overlay`

`PROJECT:FixedAspect` は `Assets/WebGLTemplates/FixedAspect/index.html` にあります。
このテンプレートは `9:16` のアスペクト比を維持し、PCブラウザでは中央にスマホ縦画面として表示します。余白は黒背景になります。

## 比率固定の仕組み

Unity側では各Canvasに `AspectSafeRootFitter` を付けています。

- `TitleScene`
- `GameScene`
- `ResultScene`

実行時にCanvas直下のUIを `AspectSafeRoot` にまとめ、1080 x 1920 の比率を維持したまま画面内に収めます。

HTML側では `#unity-canvas` に以下の方針を適用しています。

- `aspect-ratio: 9 / 16`
- `object-fit: contain`
- `max-width: 100vw`
- `max-height: 100vh`
- 中央配置

これにより、ブラウザ幅や高さが変わってもゲーム画面自体は横伸び・縦伸びしません。

## Unity Editorからビルド

1. Unity Editorでプロジェクトを開く
2. `File > Build Profiles` または `File > Build Settings` を開く
3. Platformを `WebGL` に切り替える
4. Scenes In Buildに3シーンが入っていることを確認する
5. Player SettingsでWebGL Templateが `PROJECT:FixedAspect` になっていることを確認する
6. Build先を `Builds/WebGL` などに指定してBuildする

または、追加済みメニューから実行できます。

```text
Tools > Push Notification God > Build WebGL
```

出力先は `Builds/WebGL` です。

## コマンドラインビルド

Unity Editorを閉じた状態で実行してください。同じプロジェクトをEditorで開いたままだとbatchmodeは失敗します。

```bash
/Applications/Unity/Hub/Editor/6000.4.6f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath /Users/user/Documents/UnityProjects/PushNotificationGod \
  -executeMethod PushNotificationGod.Editor.WebGLBuildScript.BuildWebGLFromCommandLine \
  -buildPath Builds/WebGL \
  -logFile Logs/WebGLBuild.log
```

`-buildPath` を省略した場合も `Builds/WebGL` に出力します。

## ローカル確認

WebGLビルドは `file://` で直接開かず、HTTPサーバー経由で確認してください。

```bash
cd Builds/WebGL
python3 -m http.server 8080
```

ブラウザで以下を開きます。

```text
http://localhost:8080
```

確認ポイント:

- PC横長画面でもゲーム画面が横に伸びない
- 9:16のスマホ縦画面として中央表示される
- 余白は黒背景になる
- スマホ縦画面で背景、ロゴ、通知カードが歪まない
- タップ、右スワイプ、スコア、コンボ、称号が動く
- BGM/SEが鳴る

## GitHub Pagesで公開

方法A: `docs/` を使う場合

1. WebGLビルドを作成する
2. ビルド成果物の中身をリポジトリ直下の `docs/` に配置する
   - `docs/index.html`
   - `docs/Build/`
   - `docs/StreamingAssets/` がある場合はそれも配置
3. GitHubの `Settings > Pages` で `Deploy from a branch` を選ぶ
4. Branchを `main`、Folderを `/docs` にする

方法B: `gh-pages` ブランチを使う場合

1. WebGLビルドを作成する
2. ビルド成果物の中身を `gh-pages` ブランチのルートに配置する
3. GitHub Pagesの公開元を `gh-pages` ブランチにする

このプロジェクトでは `Decompression Fallback` を有効にしているため、静的ホスティングでも圧縮ファイルのヘッダー問題が起きにくい構成です。

## 注意点

- Unity Editorを開いたままbatchmodeビルドしない
- HTMLテンプレートをDefaultに戻さない
- `#unity-canvas` に `width: 100%; height: 100%;` を同時指定して伸ばさない
- Canvas配下のUIを個別に画面サイズへStretchして歪ませない
- 背景Imageは `Preserve Aspect` を維持する
- WebGL公開後はスマホ実機ブラウザでも確認する

## WebGLで日本語文字が消える場合

UnityのLegacy Textは、WebGLビルド時に日本語グリフが欠けると空表示になることがあります。
このプロジェクトでは日本語対応フォントを `Resources` に同梱し、実行時に全Sceneの `UnityEngine.UI.Text` へ適用しています。

同梱フォント:

- `Assets/Resources/Fonts/HiraginoKakuGothicW6.ttc`
- `Assets/Resources/Fonts/AppleGothic.ttf`

適用処理:

- `Assets/Scripts/UI/UIJapaneseFont.cs`
- `TitleController`
- `GameManager`
- `UIManager`
- `TaskCard`
- `ResultManager`

通知本文、カウントダウン説明、設定Panel、リザルトなど、ゲーム中に必要な日本語Textもこのフォントで表示されます。
