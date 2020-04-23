using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DefaultNamespace;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(NewComponent))]
[CanEditMultipleObjects]
public class XMLEditor : Editor
{
    string _FileLocation, _FileName, _FileNameJSON, _FileNameYAML; 
    
    private void OnSceneGUI()
    {
        var node = target as NewComponent;
    }

    public override void OnInspectorGUI()
    {
        _FileLocation = Application.dataPath; 
        _FileName     = "SaveData.xml";
        _FileNameJSON = "SaveData.json";
        _FileNameYAML = "World.yaml";
        var xmlpath = _FileLocation + "\\" + _FileName;
        var jsonpath = _FileLocation + "\\" + _FileNameJSON;
        var yamlpath = _FileLocation + "\\" + _FileNameYAML;
        /*
        serializedObject.Update();
        EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();*/
        var script = target as NewComponent;

        //timescale = GUILayout.HorizontalScrollbar(timescale, 1.0f, 0.0f, 10.0f);
        GUILayout.HorizontalSlider(1, 0, 1);

        if (GUILayout.Button("Save"))
        {
            /*
            AssetDatabase.CreateAsset(XmlWriter.Create(), "Assets/MyMaterial.mat");
            //System.IO.File.WriteAllText("C:\blahblah_yourfilepath\yourtextfile.txt", firstnameGUI + ", " + lastnameGUI);
            // Print the path of the created asset
            //Debug.Log(AssetDatabase.GetAssetPath(material));*/
            
            XmlSerializer serializer = new XmlSerializer(typeof(MyData));
            TextWriter    textWriter = new StreamWriter(xmlpath);
            serializer.Serialize(textWriter, script.myData);
            textWriter.Close(); 
            
            var jsondata = JsonUtility.ToJson(script.myData, true);
            File.WriteAllText(jsonpath, jsondata);
            //XmlWriter xmlWriter = XmlWriter.Create("Assets/test.xml");
            //script.Serialize(xmlWriter);

                
            {
                //entities
                //if(World.All == null)
                if(World.All.Count<1)
                    return;
                
                var refFilePathName = @yamlpath;
            
                // Save world
                using (var writer = new StreamWriter(refFilePathName))
                {
                    writer.NewLine = "\n";
                    var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                    // Save world to yaml
                    SerializeUtility.SerializeWorldIntoYAML(em, writer, false); // is yaml just debugging?
                    
                    // Path for saving world
                    var binaryWorldPath =  _FileLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
                    var binaryWriter = new StreamBinaryWriter(binaryWorldPath);
                    
                    // Save whole world
                    // not hybrid SerializeUtility.SerializeWorld(em, binaryWriter, out var objectReferences);
                    var referencedObjectsPath =  "Assets/ReferencedUnityWorldObjects.asset";// path forward slash for asset access
                    SerializeUtilityHybrid.Serialize(em, binaryWriter, out var objectReferences);
                    
                    QueryReferences.Query(objectReferences);
                    AssetDatabase.CreateAsset(objectReferences, referencedObjectsPath);
                }
                
            }
            Debug.Log("Saved");
        }
        if (GUILayout.Button("Load"))
        {

            
            {            
                if(World.All.Count<1)
                    return;
                //entities
                var refFilePathName = @yamlpath;
            
                // To generate the file we'll test against
                var binaryPath   =  _FileLocation + "\\" + "DefaultWorld.world";
                
                // need an empty world to do this
                var world = new World("SavingWorld");
                var transaction = world.EntityManager.BeginExclusiveEntityTransaction();
                using (var reader = new StreamBinaryReader(binaryPath)) //GetFullPathByName(fileName))
                {
                    //reader.NewLine = "\n";
                    
                    var referencedObjectsPath =  "Assets/ReferencedUnityWorldObjects.asset";// path forward slash for asset access
                    var objectRefAsset = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>(referencedObjectsPath);
                    // Load objects as binary file
                    SerializeUtilityHybrid.Deserialize(world.EntityManager, reader, objectRefAsset);
                }
                
                
                World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
                World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(world.EntityManager);
            }
            Debug.Log("Loaded");
        }
        
        // Save gameobjects to binary file
        if (GUILayout.Button("Destroy GameObjects"))
        {
            
            var allGameObjects = FindObjectsOfType<GameObject>().ToList();

            script.SavedGameObjects = allGameObjects;

            GameObject ignore = null;
            foreach (var go in allGameObjects)
            {
                if (go == script.gameObject)
                {
                    ignore = go;
                }
            }
            allGameObjects.Remove(ignore);
            Debug.Log("Removing editor from destroy list");
            
            // Save objects as binary file
            var referencedObjectsPath =  "Assets/BinaryObjectsSaveFile.binary";// path forward slash for asset access
            BinarySerialization.WriteToBinaryFile(referencedObjectsPath, allGameObjects);
            Debug.Log("Saved all gameobjects to binary file");

            for (int i = 0; i < allGameObjects.Count; i++)
            {
                Destroy(allGameObjects[i]);
            }
        }
        if (GUILayout.Button("Create GameObjects"))
        {
            
            // Load objects from binary file
            var referencedObjectsPath =  "Assets/BinaryObjectsSaveFile.binary";// path forward slash for asset access
            var list = BinarySerialization.ReadFromBinaryFile<List<GameObject>>(referencedObjectsPath);
            Debug.Log("Read all gameobjects from binary file");
            
            foreach (var go in list)
            {
                Instantiate(go, go.transform.position, go.transform.rotation);
            }
        }

        if (GUILayout.Button("new save"))
        {
            WorldManagement.Save();
        }

        if (GUILayout.Button("new Load"))
        {
            WorldManagement.Load();
        }

        DrawDefaultInspector();
    }
    
    
    /*
    // Finally our save and load methods for the file itself 
    void CreateXML() 
    { 
        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath; 
        _FileName     = "SaveData.xml";
        
        StreamWriter writer; 
        FileInfo     t = new FileInfo(_FileLocation+"\\"+ _FileName); 
        if(!t.Exists) 
        { 
            writer = t.CreateText(); 
        } 
        else 
        { 
            t.Delete(); 
            writer = t.CreateText(); 
        } 
        
        writer.Write(_data); 
        writer.Close(); 
        Debug.Log("File written."); 
    } */
    
    /*
    void SaveToXml()
    {            
        string path = EditorUtility.SaveFilePanel("Save the database to XML file.", "", "ItemDatabase.xml", ".xml");            
 
        if (path == "")
            return;
           
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter writer = XmlWriter.Create(path, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("ItemDatabase");
        writer.WriteAttributeString("version", saveXMLVersion.ToString());
 
        for (int i = 0; i < database.Count; i++)
        {
            database.Get(i).writeToXML(writer);
        }
 
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        writer.Close();
    }
 
    void LoadFromXml()
    {
        string path = EditorUtility.OpenFilePanel("Select the XML file to load the database from", "", "xml");
 
        if (path == "")
            return;
 
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.Load(path);
            XmlElement  root = (XmlElement)doc.GetElementsByTagName("ItemDatabase").Item(0);
            XmlNodeList xl   = root.GetElementsByTagName("Item");
            database.Clear();
 
            foreach (XmlElement e in xl)
            {
                ISObject obj = new ISObject();
                obj.readFromXML(e);
                database.Add(obj);
            }
 
            GenerateWeaponTags();
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }*/
}


