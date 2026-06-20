async function recognize(_base64, _lang, options) {
    const { utils } = options;
    const { run, cacheDir, pluginDir } = utils;

    // Path to the screenshot that Pot captures
    const screenshot = `${cacheDir}/pot_screenshot_cut.png`;

    // PPOCRv6.exe is bundled alongside this plugin
    // Usage: PPOCRv6.exe <image_path> [tiny|small|medium]
    const result = await run(
        `${pluginDir}/PPOCRv6.exe`,
        [screenshot, "small"]
    );

    if (result.status !== 0) {
        throw new Error(result.stderr || `Exit code: ${result.status}`);
    }

    const stdout = result.stdout.trim();
    if (!stdout) {
        throw new Error("Empty output from OCR engine");
    }

    let json;
    try {
        json = JSON.parse(stdout);
    } catch (e) {
        throw new Error(`Invalid JSON output: ${stdout.substring(0, 200)}`);
    }

    if (json.code !== 100 || !json.data || json.data.length === 0) {
        throw new Error(json.error || "No text recognized");
    }

    // Join all detected text lines
    return json.data
        .map(item => item.text)
        .filter(t => t && t.trim())
        .join("\n")
        .trim();
}
