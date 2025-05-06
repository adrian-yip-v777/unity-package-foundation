using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace vz777.Foundation.Editor
{
    public class MergeSubMeshesWindow : EditorWindow
    {
        private GameObject _targetObject;
        private readonly List<MeshFilter> _meshesToMerge = new();
        private Vector2 _meshesToMergeScroll;
        private bool _replaceOriginalMesh = true;

        [MenuItem("Tools/vz777/Merge Sub-Meshes")]
        public static void ShowWindow()
        {
            GetWindow<MergeSubMeshesWindow>("Merge Sub-Meshes");
        }

        private void OnGUI()
        {
            // Display help box with detailed information
            EditorGUILayout.HelpBox(
                "The tool will find all MeshFilters in its hierarchy that have meshes with multiple sub-meshes. " +
                "Clicking 'Merge Sub-Meshes' will create new mesh assets with all sub-meshes combined into one and assign them to the scene instance. " +
                "Note that this will result in the entire mesh using only the first material, which might affect rendering if different parts were using different materials. ",
                MessageType.Info);

            // Display dragged object
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            _targetObject = EditorGUILayout.ObjectField(_targetObject, typeof(GameObject), true) as GameObject;
            
            if (!_targetObject) return;

            // Toggle for replacing original mesh in scene
            EditorGUILayout.BeginHorizontal();
            _replaceOriginalMesh = EditorGUILayout.Toggle(_replaceOriginalMesh, GUILayout.MaxWidth(16));
            EditorGUILayout.LabelField("Replace mesh in target mesh filter", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        
            // Find all MeshFilters with multiple sub-meshes
            _meshesToMerge.Clear();
            
            var filters = _targetObject.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                if (filter.sharedMesh && filter.sharedMesh.vertices != null && filter.sharedMesh.subMeshCount > 1)
                {
                    _meshesToMerge.Add(filter);
                }
                else if (!filter.sharedMesh || filter.sharedMesh.vertices == null || filter.sharedMesh.subMeshCount <= 0)
                {
                    Debug.LogWarning($"MeshFilter on GameObject '{filter.gameObject.name}' has a null or invalid mesh (Name: {(filter.sharedMesh != null ? filter.sharedMesh.name : "null")}, InstanceID: {(filter.sharedMesh != null ? filter.sharedMesh.GetInstanceID() : 0)}) and will be skipped.", filter.gameObject);
                }
            }
            
            // Display meshes with multiple sub-meshes in a scrollable view
            EditorGUILayout.LabelField("Meshes with multiple sub-meshes:", EditorStyles.boldLabel);
            _meshesToMergeScroll = EditorGUILayout.BeginScrollView(_meshesToMergeScroll, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical();
            foreach (var filter in _meshesToMerge)
            {
                EditorGUILayout.ObjectField(filter.gameObject, typeof(GameObject), true);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"Number of meshes to merge: {_meshesToMerge.Count}", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            if (_meshesToMerge.Count == 0 || !_targetObject)
            {
                EditorGUILayout.LabelField("No valid GameObject or meshes with multiple sub-meshes found.");
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            // Tools
            if (GUILayout.Button("Generate Analysis CSV"))
            {
                GenerateSubMeshListCsv();
            }
            
            // Merge button
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Merge", GUILayout.ExpandWidth(true)))
            {
                MergeAllSubMeshes();
                ShowNotification(new GUIContent("Sub-meshes merged successfully. New meshes created in selected folder."));
            }
            GUI.backgroundColor = originalColor;
            
            EditorGUILayout.EndHorizontal();
        }

        private void MergeAllSubMeshes()
        {
            // Prompt user to select folder for merged meshes
            var mergeFolder = EditorUtility.SaveFolderPanel("Select Folder for Merged Meshes", "Assets", "");
            if (string.IsNullOrEmpty(mergeFolder) || !mergeFolder.StartsWith(Application.dataPath))
            {
                Debug.LogWarning("Invalid or out-of-project folder selected. Merging aborted.");
                return;
            }
            mergeFolder = "Assets" + mergeFolder.Substring(Application.dataPath.Length).TrimStart('/');

            // Use existing folder or create it only if it doesn't exist
            if (!AssetDatabase.IsValidFolder(mergeFolder))
            {
                string parentPath = Path.GetDirectoryName(mergeFolder);
                string folderName = Path.GetFileName(mergeFolder);
                if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
                {
                    Debug.LogWarning($"Parent path '{parentPath}' is invalid. Creating full path.");
                    AssetDatabase.CreateFolder("Assets", folderName); // Create at root if parent invalid
                }
                else if (!string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }
                else
                {
                    Debug.LogWarning("Invalid folder name derived from selection. Merging aborted.");
                    return;
                }
            }

            // Revalidate meshes to merge to ensure consistency
            var validMeshesToMerge = new List<MeshFilter>();
            foreach (var filter in _meshesToMerge)
            {
                if (filter.sharedMesh && filter.sharedMesh.vertices != null && filter.sharedMesh.subMeshCount > 1)
                {
                    validMeshesToMerge.Add(filter);
                }
                else
                {
                    Debug.LogWarning($"MeshFilter on GameObject '{filter.gameObject.name}' is no longer valid (Name: {(filter.sharedMesh != null ? filter.sharedMesh.name : "null")}, InstanceID: {(filter.sharedMesh != null ? filter.sharedMesh.GetInstanceID() : 0)}) and will be skipped during merge.", filter.gameObject);
                }
            }

            // Find all unique shared meshes that need merging
            var uniqueMeshes = new HashSet<Mesh>();
            foreach (var filter in validMeshesToMerge)
            {
                if (!filter.sharedMesh || filter.sharedMesh.vertices == null || filter.sharedMesh.subMeshCount <= 1)
                {
                    Debug.LogWarning($"Skipping mesh in uniqueMeshes collection for GameObject '{filter.gameObject.name}' due to invalid state (Name: {(filter.sharedMesh != null ? filter.sharedMesh.name : "null")}, InstanceID: {(filter.sharedMesh != null ? filter.sharedMesh.GetInstanceID() : 0)}).", filter.gameObject);
                    continue;
                }
                
                uniqueMeshes.Add(filter.sharedMesh);
                Debug.Log($"Processing mesh: Name: {filter.sharedMesh.name}, InstanceID: {filter.sharedMesh.GetInstanceID()}", filter.sharedMesh);
            }

            // Create merged meshes for each unique shared mesh
            var mergedMeshes = new Dictionary<Mesh, Mesh>();
            foreach (var originalMesh in uniqueMeshes)
            {
                // Create new mesh
                var newMesh = new Mesh
                {
                    name = originalMesh.name + "_merged",
                    // Copy data
                    vertices = originalMesh.vertices,
                    normals = originalMesh.normals
                };

                if (originalMesh.tangents != null) newMesh.tangents = originalMesh.tangents;
                newMesh.uv = originalMesh.uv;
                if (originalMesh.uv2 != null) newMesh.uv2 = originalMesh.uv2;

                // Combine triangles
                var allTriangles = new List<int>();
                for (var submesh = 0; submesh < originalMesh.subMeshCount; submesh++)
                {
                    allTriangles.AddRange(originalMesh.GetTriangles(submesh));
                }
                newMesh.SetTriangles(allTriangles, 0);
                newMesh.subMeshCount = 1;

                // Save as asset with unique path
                var basePath = Path.Combine(mergeFolder, newMesh.name + ".asset");
                var uniquePath = AssetDatabase.GenerateUniqueAssetPath(basePath);
                AssetDatabase.CreateAsset(newMesh, uniquePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                mergedMeshes[originalMesh] = newMesh;
                Debug.Log($"Created merged mesh: Name: {newMesh.name}, InstanceID: {newMesh.GetInstanceID()}, Path: {uniquePath}", newMesh);
            }

            // Assign new meshes to MeshFilters in the scene instance
            foreach (var filter in validMeshesToMerge)
            {
                if (!filter.sharedMesh || filter.sharedMesh.vertices == null || filter.sharedMesh.subMeshCount <= 0)
                {
                    Debug.LogWarning($"MeshFilter on GameObject '{filter.gameObject.name}' has a null or invalid mesh (Name: {(filter.sharedMesh ? filter.sharedMesh.name : "null")}, InstanceID: {(filter.sharedMesh ? filter.sharedMesh.GetInstanceID() : 0)}) and will be skipped.", filter.gameObject);
                    continue;
                }

                if (mergedMeshes.ContainsKey(filter.sharedMesh))
                {
                    if (_replaceOriginalMesh)
                    {
                        Debug.Log($"Assigning merged mesh to GameObject '{filter.gameObject.name}' in scene: Original Mesh Name: {filter.sharedMesh.name}, InstanceID: {filter.sharedMesh.GetInstanceID()}", filter.gameObject);
                        filter.sharedMesh = mergedMeshes[filter.sharedMesh];
                        // Update MeshCollider if present
                        var collider = filter.GetComponent<MeshCollider>();
                        if (collider)
                        {
                            collider.sharedMesh = mergedMeshes[filter.sharedMesh];
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No merged mesh found for MeshFilter on GameObject '{filter.gameObject.name}' (Mesh Name: {filter.sharedMesh.name}, InstanceID: {filter.sharedMesh.GetInstanceID()}). Mesh may not have been processed.", filter.gameObject);
                }
            }

            // Final save and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateSubMeshListCsv()
        {
            // CSV header
            var csvContent = "Object Name,Relative Path,SubMesh Count\n";

            // Collect data for each MeshFilter with multiple sub-meshes
            foreach (var filter in _meshesToMerge)
            {
                var relativePath = GetRelativePath(filter.gameObject, _targetObject);
                var subMeshCount = filter.sharedMesh.subMeshCount;
                csvContent += $"{filter.gameObject.name},{relativePath},{subMeshCount}\n";
            }

            // Prompt user to save the CSV file
            var defaultPath = EditorUtility.SaveFilePanel("Save Sub-Mesh List as CSV", "", "submesh_list.csv", "csv");
            if (string.IsNullOrEmpty(defaultPath)) return;

            if (!Directory.Exists(defaultPath))
                Directory.CreateDirectory(defaultPath);
            
            File.WriteAllText(defaultPath, csvContent);
            Debug.Log($"Sub-mesh list saved to: {defaultPath}");
        }

        private string GetRelativePath(GameObject target, GameObject root)
        {
            if (!target || !root)
                return "";

            var pathSegments = new List<string>();
            var current = target.transform;

            // Traverse up the hierarchy until we reach the root
            while (current && current.gameObject != root)
            {
                pathSegments.Insert(0, current.gameObject.name);
                current = current.parent;
            }

            // If we didn't reach the root, return an empty path
            return current ? string.Join("/", pathSegments) : "";
        }
    }
}