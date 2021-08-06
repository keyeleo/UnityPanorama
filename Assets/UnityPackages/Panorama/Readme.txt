插件功能：扫描建模、全景图资源管理
使用：
0，加载obj场景，需要设置：Rotation.x=-90，Scale.x=-1，Scale.y=-1；
1，将PanoScene.cs组件绑定到obj模型上，并为其绑定prefab、settings、textures等资源；
2，如果使用输入控制，将InputComponent、CameraController组件绑定到Main Camera；