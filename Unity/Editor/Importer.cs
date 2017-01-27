using UnityEngine;
using UnityEditor;
using System.IO;

namespace Nima.Unity
{
	public class Importer : AssetPostprocessor
	{
		static Material GetMaterial(string name)
		{
			// Make sure Texture is available too.
			string texturePath = name + ".png";
			string materialPath = name + ".mat";

			Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

			TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if(texture == null || textureImporter == null)
			{
				Debug.Log("Nima importer - missing texture at path " + texturePath);
				return null;
			}
			textureImporter.textureType = TextureImporterType.Default;
			//textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			textureImporter.mipmapEnabled = false;
			textureImporter.alphaIsTransparency = true;
			textureImporter.maxTextureSize = 2048;

			EditorUtility.SetDirty(textureImporter);
			AssetDatabase.ImportAsset(texturePath);

			Material m = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
			if(m == null)
			{
				m = new Material(Shader.Find("Unlit/Transparent"));
				AssetDatabase.CreateAsset(m, materialPath);
			}

			m.mainTexture = texture;
			EditorUtility.SetDirty(m);
			AssetDatabase.SaveAssets();
			return m;
		}

		static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths)
		{
			foreach(string path in imported)
			{

				string extension = Path.GetExtension(path).ToLower();
				string filename = Path.GetFileNameWithoutExtension(path);
				string secondExtension = Path.GetExtension(filename);

				// This is our identifier for a Nima Character being added to the assets.
				// Due to how Unity implements custom assets, we have to use the .bytes extension to support it being accessed as a TextAsset type.
				if(secondExtension == ".nima" && extension == ".bytes")
				{
					// Get filename without .nima.bytes
					filename = Path.GetFileNameWithoutExtension(filename);

					TextAsset rawAsset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
					if(rawAsset != null)
					{
						string assetPath = Path.GetDirectoryName(path) + "/" + filename + ".asset";
						// Load or make the asset that will be responsible for loading and sharing nodes, meshes, etc across all game object instances.
						ActorAsset actorAsset = (ActorAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ActorAsset));
						if(actorAsset != null)
						{
							Debug.Log("RELOADING ACTOR ASSET");
							// If the load succeeds, update the GameObjects that are referencing this asset.
							if(actorAsset.Load(rawAsset))
							{
								// Do we really want to do this? If the user had customized the character, the customizations would get blown away.
								// Leave it out for now.
								//UpdateGameObjectsFor(actorAsset);
							}
						}
						else
						{
							Debug.Log("LOADING ACTOR ASSET");
							actorAsset = ScriptableObject.CreateInstance<ActorAsset>();
							if(actorAsset.Load(rawAsset))
							{
								AssetDatabase.CreateAsset(actorAsset, assetPath);
								if(actorAsset.Load(rawAsset))
								{
									AssetDatabase.SaveAssets();
								}
							}
						}

						// Finally sync up the materials.
						if(actorAsset != null)
						{
							int neededMaterials = actorAsset.Actor.TexturesUsed;

							if(actorAsset.m_TextureMaterials == null || actorAsset.m_TextureMaterials.Length != neededMaterials)
							{
								actorAsset.m_TextureMaterials = new Material[neededMaterials];
							}
							if(neededMaterials == 1)
							{
								Material m = GetMaterial(Path.GetDirectoryName(path) + "/" + filename);
								actorAsset.m_TextureMaterials[0] = m;
							}
							else
							{
								for(int i = 0; i < neededMaterials; i++)
								{
									Material m = GetMaterial(Path.GetDirectoryName(path) + "/" + filename + i);
									actorAsset.m_TextureMaterials[i] = m;
								}
							}
							// Force saving the asset.
							EditorUtility.SetDirty(actorAsset);
						}
					}
					/*Debug.Log("Got NIMA " + path);
					using (FileStream fs = new FileStream(path, FileMode.Open))
					{
						using (BinaryReader reader = new BinaryReader (fs))
						{
							ActorAsset actorAsset = (ActorAsset)AssetDatabase.LoadAssetAtPath(path + ".asset", typeof(ActorAsset));
							// Maybe always overwrite here?
							if(actorAsset != null)
							{
								Debug.Log("Updating existing actor.");
								actorAsset.Load(reader);
							}
							else
							{
								Debug.Log("Loading new actor.");
								actorAsset = ActorAsset.CreateInstance<ActorAsset>();
								if(actorAsset.Load(reader))
								{
									AssetDatabase.CreateAsset(actorAsset, path + ".asset");
									actorAsset.m_Asset = 
									AssetDatabase.SaveAssets();
								}
							}
						}
					}*/
				}
			}
		}

		private static void UpdateGameObjectsFor(ActorAsset actorAsset)
		{
			Object[] list = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach(Object obj in list)
			{
				GameObject go = obj as GameObject;
				ActorComponent actor = go.GetComponent<ActorComponent>();
				if(actor != null && actor.Asset == actorAsset)
				{
					Debug.Log("FOUND ACTOR WITH UPDATED ASSET");
					// We found an actor using the asset that got updated. Let's update the game object.
					actor.InitializeFromAsset(actorAsset);
				}
			}
		}

		[MenuItem("Assets/Nima/Instance Actor", false, 1)]
		static void InstanceActor () 
		{
			foreach (object obj in Selection.objects) 
			{
				ActorAsset actorAsset = obj as ActorAsset;

				string actorInstanceName = actorAsset.name;
				GameObject go = new GameObject(actorInstanceName, typeof(ActorComponent));
				go.GetComponent<ActorComponent>().SetActorAsset(actorAsset);
			}
		}

		[MenuItem("Assets/Nima/Instance Actor", true, 1)]
		static bool ValidateInstanceActor () 
		{
			foreach (object o in Selection.objects) 
			{
				if (o.GetType() != typeof(ActorAsset))
				{
					return false;
				}
			}
			return true;
		}
	}
}