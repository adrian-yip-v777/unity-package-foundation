using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace vz777.Foundation.Editor
{
    public class ExportGitIgnoreFilesTool : EditorWindow
    {
        private readonly List<string> _ignoredPatterns = new();
        private readonly Dictionary<string, List<FileInfo>> _filesByExtension = new();
        private readonly Dictionary<string, bool> _extensionToggles = new();
        private readonly HashSet<string> _excludedFiles = new();
        private VisualElement _fileListContainer;
        private Label _sizeLabel;
        private TextField _searchField;
        private string _searchQuery = "";
        private Label _noFilesLabel;

        [MenuItem("Tools/vz777/Export .gitignore files")]
        public static void ShowWindow()
        {
            var window = GetWindow<ExportGitIgnoreFilesTool>("Export .gitignore files Tool");
            window.minSize = new Vector2(400, 500);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.flexDirection = FlexDirection.Column;

            var styleSheet = EditorGUIUtility.Load("StyleSheets/Editor/Standard.uss") as StyleSheet;
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);

            LoadGitignorePatterns();
            ScanIgnoredFiles();

            var toggleContainer = new VisualElement
            {
                style = { marginBottom = 10, flexShrink = 0 }
            };
            foreach (var ext in _filesByExtension.Keys.OrderBy(k => k))
            {
                var toggle = new Toggle(ext)
                {
                    value = true,
                    style = { marginBottom = 2 }
                };
                toggle.RegisterValueChangedCallback(evt => OnToggleChanged(ext, evt.newValue));
                toggleContainer.Add(toggle);
                _extensionToggles[ext] = true;
            }
            root.Add(toggleContainer);

            var fileListSection = new VisualElement
            {
                style = { flexGrow = 1, marginBottom = 10 }
            };
            _searchField = new TextField
            {
                style = { marginBottom = 5 }
            };
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue.ToLower();
                UpdateFileList();
            });
            fileListSection.Add(_searchField);

            _fileListContainer = new VisualElement();
            var fileScrollView = new ScrollView
            {
                style = { flexGrow = 1 }
            };
            fileScrollView.Add(_fileListContainer);
            fileListSection.Add(fileScrollView);
            root.Add(fileListSection);

            _noFilesLabel = new Label("No ignored files found in Assets.")
            {
                style = { display = DisplayStyle.None, color = Color.gray, marginTop = 10 }
            };
            _fileListContainer.Add(_noFilesLabel);

            UpdateFileList();

            _sizeLabel = new Label("Estimated Package Size: Calculating...")
            {
                style = { marginBottom = 10 }
            };
            root.Add(_sizeLabel);
            UpdateSizeEstimate();

            var buttonContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, flexShrink = 0 }
            };
            var exportButton = new Button(OnExportClicked) { text = "Export Package" };
            var resetButton = new Button(OnResetClicked) { text = "Reset" };
            buttonContainer.Add(exportButton);
            buttonContainer.Add(resetButton);
            root.Add(buttonContainer);
        }

        private void LoadGitignorePatterns()
        {
            _ignoredPatterns.Clear();
            var gitignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");
            if (!File.Exists(gitignorePath))
                return;

            foreach (var line in File.ReadAllLines(gitignorePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;
                _ignoredPatterns.Add(trimmed);
            }
        }

        private void ScanIgnoredFiles()
        {
            _filesByExtension.Clear();
            _extensionToggles.Clear();
            _excludedFiles.Clear();

            var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
            if (!Directory.Exists(assetsPath))
                return;

            foreach (var file in Directory.GetFiles(assetsPath, "*", SearchOption.AllDirectories))
            {
                if (!IsFileIgnored(file)) continue;
                
                var fileInfo = new FileInfo(file);
                var ext = fileInfo.Extension.ToLower();
                if (string.IsNullOrEmpty(ext))
                    ext = "no_extension";

                if (!_filesByExtension.ContainsKey(ext))
                    _filesByExtension[ext] = new List<FileInfo>();
                _filesByExtension[ext].Add(fileInfo);
            }
        }

        private bool IsFileIgnored(string filePath)
        {
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace("\\", "/").ToLower();

            foreach (var pattern in _ignoredPatterns)
            {
                var normalizedPattern = pattern.Trim().ToLower();
                if (normalizedPattern.StartsWith("/"))
                    normalizedPattern = normalizedPattern.Substring(1);

                string regexPattern;
                if (normalizedPattern.EndsWith("/"))
                {
                    regexPattern = "^" + Regex.Escape(normalizedPattern.TrimEnd('/')) + "(/.*)?$";
                }
                else if (normalizedPattern.Contains("*") || normalizedPattern.Contains("?"))
                {
                    var escapedPattern = Regex.Escape(normalizedPattern).Replace("\\*", ".*").Replace("\\?", ".");
                    regexPattern = escapedPattern.Replace("\\[", "[").Replace("\\]", "]");
                    if (!regexPattern.EndsWith(".*"))
                        regexPattern += "(/.+)?$";
                }
                else
                {
                    regexPattern = normalizedPattern.Contains("/") 
                        ? "^" + Regex.Escape(normalizedPattern) + "(/.*)?$" 
                        : ".*/" + Regex.Escape(normalizedPattern) + "$";
                }

                if (Regex.IsMatch(relativePath, regexPattern, RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        private void OnToggleChanged(string extension, bool value)
        {
            _extensionToggles[extension] = value;
            UpdateFileList();
            UpdateSizeEstimate();
        }

        private void UpdateFileList()
        {
            _fileListContainer.Clear();
            _fileListContainer.Add(_noFilesLabel);
            var fileCount = 0;

            foreach (var ext in _filesByExtension.Keys.OrderBy(k => k))
            {
                if (!_extensionToggles[ext])
                    continue;

                var masterToggle = new Toggle
                {
                    value = _filesByExtension[ext].All(f => !_excludedFiles.Contains(f.FullName))
                };
                masterToggle.RegisterValueChangedCallback(evt =>
                {
                    foreach (var file in _filesByExtension[ext])
                    {
                        if (evt.newValue)
                            _excludedFiles.Remove(file.FullName);
                        else
                            _excludedFiles.Add(file.FullName);
                    }
                    // Schedule the update to avoid recursive loop
                    EditorApplication.delayCall += () =>
                    {
                        UpdateFileList();
                        UpdateSizeEstimate();
                    };
                });

                var extLabel = new Label(ext.ToUpper())
                {
                    style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 5 }
                };

                var extContainer = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                };
                extContainer.Add(masterToggle);
                extContainer.Add(extLabel);
                _fileListContainer.Add(extContainer);

                foreach (var file in _filesByExtension[ext])
                {
                    var relativePath = Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "Assets"), file.FullName);
                    if (!string.IsNullOrEmpty(_searchQuery) && !relativePath.ToLower().Contains(_searchQuery))
                        continue;

                    var isIncluded = !_excludedFiles.Contains(file.FullName);
                    var toggle = new Toggle
                    {
                        value = isIncluded,
                        style = { marginRight = 10 }
                    };
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue)
                            _excludedFiles.Remove(file.FullName);
                        else
                            _excludedFiles.Add(file.FullName);
                        EditorApplication.delayCall += () =>
                        {
                            UpdateFileList();
                            UpdateSizeEstimate();
                        };
                    });

                    var fileContainer = new VisualElement
                    {
                        style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginLeft = 20, marginBottom = 2 }
                    };
                    var pathLabel = new Label(relativePath);
                    fileContainer.Add(pathLabel);
                    fileContainer.Add(toggle);
                    _fileListContainer.Add(fileContainer);
                    fileCount++;
                }

                var includedCount = _filesByExtension[ext].Count(f => !_excludedFiles.Contains(f.FullName));
                if (includedCount == _filesByExtension[ext].Count)
                    masterToggle.value = true;
                else if (includedCount == 0)
                    masterToggle.value = false;
                else
                {
                    masterToggle.style.opacity = 0.5f;
                }
            }

            _noFilesLabel.style.display = fileCount == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateSizeEstimate()
        {
            long totalSize = 0;
            foreach (var ext in _filesByExtension.Keys)
            {
                if (!_extensionToggles[ext])
                    continue;

                foreach (var file in _filesByExtension[ext])
                {
                    if (!_excludedFiles.Contains(file.FullName))
                        totalSize += file.Length;
                }
            }

            _sizeLabel.text = $"Estimated Package Size: {FormatFileSize(totalSize)}";
        }

        private string FormatFileSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            var unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:F2} {units[unitIndex]}";
        }

        private void OnExportClicked()
        {
            var exportPath = EditorUtility.SaveFilePanel("Export Unity Package", "", "ExportedAssets.unitypackage", "unitypackage");
            if (string.IsNullOrEmpty(exportPath))
                return;

            var assetPaths = new List<string>();
            foreach (var ext in _filesByExtension.Keys)
            {
                if (!_extensionToggles[ext])
                    continue;

                foreach (var file in _filesByExtension[ext])
                {
                    if (_excludedFiles.Contains(file.FullName)) continue;
                    var relativePath = "Assets/" + Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "Assets"), file.FullName).Replace("\\", "/");
                    var metaPath = relativePath + ".meta";
                    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), metaPath)))
                    {
                        assetPaths.Add(relativePath);
                    }
                }
            }

            if (assetPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "No valid Unity assets selected for export. Ensure selected files are Unity assets (e.g., have a .meta file).", "OK");
                return;
            }

            AssetDatabase.ExportPackage(assetPaths.ToArray(), exportPath, ExportPackageOptions.Default);
            EditorUtility.RevealInFinder(exportPath);
        }

        private void OnResetClicked()
        {
            foreach (var ext in _extensionToggles.Keys.ToList())
                _extensionToggles[ext] = true;
            _excludedFiles.Clear();
            _searchField.value = "";
            _searchQuery = "";
            UpdateFileList();
            UpdateSizeEstimate();
        }
    }
}