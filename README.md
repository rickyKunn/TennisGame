#  Online 3D Tennis Game

![Unity](https://img.shields.io/badge/Unity-black?logo=unity&logoColor=white) ![Photon Fusion](https://img.shields.io/badge/Photon%20Fusion-blueviolet?logo=unity&logoColor=white)

---

## プロジェクト概要

**Online 3D Tennis Game** は、Unity と Photon Fusion を用いて開発した、リアルタイム・オンライン対戦対応の 3D テニスゲームです。  
同じルーム名でマッチングしたプレイヤー同士が、ネットワークを介してラリーを楽しむことができます。  


このプロジェクトは**UnityRoom**に公開しており[こちらからプレイできます。](https://unityroom.com/games/rickytennisgame)

なお、**2人プレイ専用**となっており、同じルームにプレイヤーが2人いる状態でないとスタートできない仕様となっております。

---
## プレイ動画

[![プレイ動画](docs/TennisGameSourceImage.png)](https://www.youtube.com/watch?v=bSSGPrRS-Fc)

---

## 主な機能

- **オンラインマルチプレイヤー**  
  - Photon Fusion で安定したデータ同期  
  - 1vs1 マッチングロビー／対戦開始  
- **操作**
  - サービス側が`Shift`でセット開始
  - `W` `A` `S` `D`で移動
  - `方向キー`で視点移動
  - `Shift`でトス
  - `Shift`でドライブショット
  - `Space`でスライスショット
  - `Z`でドロップショット
  - `Control`でロブショット
- **コツ**
  - サービスは、**ドライブ**と**スライス**があり、ボールの最高到達点に近いとボールの威力が上がる。
  - ショットは、プレイヤーに近いとボールの威力が上がる。
- **キャラクター**
  - キャラクターは**4種類**で、ショットの強さや走る速度、スライスの回転威力など、さまざまな特徴がある。 
- **スコア表示＆ゲーム終了判定**  
  - リアルタイムでスコアボード更新  
  - 1 セット先取でサービス交代
---

## 使用技術

- **ゲームエンジン**：Unity
- **ネットワーク**：Photon Fusion  
- **プログラミング言語**：C# (Unity)

---
## ライセンスおよびクレジット

このリポジトリは、ポートフォリオおよび選考目的での閲覧専用として公開しています。

- 商用利用は禁止です  
- 改変や再配布は禁止です  
- 閲覧および参照は自由ですが、許可なく他用途に使用しないでください

その他の目的での使用を希望される場合は、下記までご連絡ください。  
**kobayashiritsuki@gmail.com**

---

本プロジェクトでは、ユニティ・テクノロジーズ・ジャパンが提供する「Unity-Chan」キャラクターおよび関連アセットを使用しています。  
使用にあたっては、以下のライセンスに従っています：  
[Unity-Chan ライセンス（日本語）](https://unity-chan.com/contents/license_jp/)  
[Unity-Chan License (English)](https://unity-chan.com/contents/license_en/)

© 2025 小林立樹  
© Unity Technologies Japan / Unity-Chan Project. All rights reserved.
