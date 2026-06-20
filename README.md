# Pot-App PP-OCRv6 ONNX 插件

基于百度 PP-OCRv6 引擎的 Pot 文字识别插件，C# .NET 9 实现，零依赖。

## 特点

- **PP-OCRv6 small 模型**：检测 84.1% + 识别 81.3%，碾压百亿参数大模型
- **零依赖**：捆绑 .NET 运行时 + VC++ 运行时 + ONNX Runtime，解压即用
- **支持 50 种语言**：中、英、日、韩及 46 种拉丁语系
- **离线运行**：无需联网，所有推理在本地完成

## 安装

1. 从 [Releases](https://github.com/zhp2333/pot-app-recognize-plugin-ppocrv6/releases) 下载 `plugin.com.hermes.ppocrv6.potext`
2. 打开 Pot → 偏好设置 → 服务设置 → 文字识别 → 添加外部插件 → 安装外部插件
3. 选择 `.potext` 文件安装
4. 在 OCR 服务列表中将「PP-OCRv6 (ONNX)」拖到首位

## 使用

- `Ctrl+]` 框选文字区域即可识别
- 中文场景准确率远超系统 OCR
- 英文场景建议搭配 Tesseract 插件使用

## 速度

冷启动约 2 秒（含 ONNX Runtime 初始化和模型加载）。

## 技术栈

- C# .NET 9
- [RapidOCRSharpOnnx](https://github.com/meloht/RapidOCRSharpOnnx) 1.2.2
- [Microsoft.ML.OnnxRuntime](https://github.com/microsoft/onnxruntime) 1.27.0
- [OpenCvSharp4](https://github.com/shimat/opencvsharp)

## 构建

```bash
# 1. 安装 .NET 9 SDK
# 2. 下载模型
mkdir -p models
curl -L -o models/ch_PP-OCRv6_small_det.onnx \
  "https://modelscope.cn/models/PaddlePaddle/PP-OCRv6_small_det_onnx/resolve/master/inference.onnx"
curl -L -o models/ch_PP-OCRv6_small_rec.onnx \
  "https://modelscope.cn/models/PaddlePaddle/PP-OCRv6_small_rec_onnx/resolve/master/inference.onnx"

# 3. 编译
dotnet publish -c Release --self-contained true -r win-x64 \
  -p:PublishSingleFile=false -p:PublishTrimmed=false -o publish

# 4. 打包插件
# 将 main.js, info.json, icon.png, publish/*, models/*.onnx 打包为 .potext (zip)
```

## License

MIT
