using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickEditor
{
    /// <summary>
    /// Configuration settings for QuickEditor
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Enable debug logging
        /// </summary>
        [JsonPropertyName("enableLogging")]
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// Log file path (relative to executable directory)
        /// </summary>
        [JsonPropertyName("logFilePath")]
        public string LogFilePath { get; set; } = "quickeditor.log";

        /// <summary>
        /// Log level: Debug, Info, Warning, Error
        /// </summary>
        [JsonPropertyName("logLevel")]
        public string LogLevel { get; set; } = "Debug";

        /// <summary>
        /// Editor window width
        /// </summary>
        [JsonPropertyName("editorWidth")]
        public double EditorWidth { get; set; } = 600;

        /// <summary>
        /// Editor window height
        /// </summary>
        [JsonPropertyName("editorHeight")]
        public double EditorHeight { get; set; } = 300;

        /// <summary>
        /// Window transparency (0.0 = fully transparent, 1.0 = fully opaque)
        /// </summary>
        [JsonPropertyName("windowOpacity")]
        public double WindowOpacity { get; set; } = 0.9;

        /// <summary>
        /// Enable console logging in Monaco Editor
        /// </summary>
        [JsonPropertyName("enableConsoleLogging")]
        public bool EnableConsoleLogging { get; set; } = false;

        private static readonly string ConfigFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
            "config.json"
        );

        /// <summary>
        /// Load configuration from file or create default
        /// </summary>
        public static Config Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
                else
                {
                    var config = new Config();
                    config.Save(); // Create default config file
                    return config;
                }
            }
            catch (Exception ex)
            {
                // Can't use Logger here as it might not be initialized yet
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
                return new Config();
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                // Can't use Logger here as it might not be initialized yet
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}