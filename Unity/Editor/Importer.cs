using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

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
			textureImporter.mipmapEnabled = true;
			textureImporter.alphaIsTransparency = false;
			textureImporter.maxTextureSize = 2048;

			EditorUtility.SetDirty(textureImporter);
			AssetDatabase.ImportAsset(texturePath);

			Material m = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
			if(m == null)
			{
				m = new Material(Shader.Find("Nima/Normal"));
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
				if(
					(secondExtension == ".nma" && extension == ".bytes") ||
					(secondExtension == ".nima" && extension == ".bytes"))
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
							//Debug.Log("RELOADING ACTOR ASSET");
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
							//Debug.Log("LOADING ACTOR ASSET");
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

		// [MenuItem("Assets/Nima/Instance Actor", false, 1)]
		// static void InstanceActor () 
		// {
		// 	foreach (object obj in Selection.objects) 
		// 	{
		// 		ActorAsset actorAsset = obj as ActorAsset;

		// 		string actorInstanceName = actorAsset.name;
		// 		GameObject go = new GameObject(actorInstanceName, typeof(ActorComponent));
		// 		go.GetComponent<ActorComponent>().SetActorAsset(actorAsset);
		// 	}
		// }

		// [MenuItem("Assets/Nima/Instance Actor", true, 1)]
		// static bool ValidateInstanceActor () 
		// {
		// 	foreach (object o in Selection.objects) 
		// 	{
		// 		if (o == null || o.GetType() != typeof(ActorAsset))
		// 		{
		// 			return false;
		// 		}
		// 	}
		// 	return true;
		// }

		[MenuItem("Assets/Nima/Instance Actor", false, 1)]
		static void InstanceSingleMeshActor () 
		{
			foreach (object obj in Selection.objects) 
			{
				ActorAsset actorAsset = obj as ActorAsset;

				string actorInstanceName = actorAsset.name;
				GameObject go = new GameObject(actorInstanceName, typeof(MeshFilter), typeof(MeshRenderer), typeof(ActorSingleMeshComponent));
				go.GetComponent<ActorSingleMeshComponent>().SetActorAsset(actorAsset);
			}
		}

		[MenuItem("Assets/Nima/Instance Actor", true, 1)]
		static bool ValidateInstanceSingleMeshActor () 
		{
			foreach (object o in Selection.objects) 
			{
				if (o == null || o.GetType() != typeof(ActorAsset))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem("Assets/Nima/Instance Canvas Actor", false, 1)]
		static void InstanceCanvasActor () 
		{
			foreach (object obj in Selection.objects) 
			{
				ActorAsset actorAsset = obj as ActorAsset;

				string actorInstanceName = actorAsset.name;
				GameObject go = new GameObject(actorInstanceName, typeof(RectTransform), typeof(ActorCanvasComponent));
				go.GetComponent<ActorCanvasComponent>().SetActorAsset(actorAsset);
			}
		}

		[MenuItem("Assets/Nima/Instance Canvas Actor", true, 1)]
		static bool ValidateInstanceCanvasActor () 
		{
			foreach (object o in Selection.objects) 
			{
				if (o == null || o.GetType() != typeof(ActorAsset))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem("Assets/Nima/Make Mecanim Controller", false, 1)]
		static void MakeMecanimController() 
		{
			foreach (object obj in Selection.objects) 
			{
				ActorAsset actorAsset = obj as ActorAsset;
				string actorPath = AssetDatabase.GetAssetPath(actorAsset);
				actorPath = actorPath.Replace(".asset", "");
				Debug.Log("Actor Path " + actorPath);
				int idx = 0;
				string filename = null;
				while (true) 
				{
					filename = idx == 0 ? actorPath + ".controller" : actorPath + "_" + idx + ".controller";
					if(!File.Exists(filename))
					{
						break;
					}
					idx++;
				}

				UnityEditor.Animations.AnimatorController animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(filename) as UnityEditor.Animations.AnimatorController;
				if(!actorAsset.Loaded)
				{
					actorAsset.Load();
				}
				Nima.Actor actor = actorAsset.Actor;
				foreach(Nima.Animation.ActorAnimation actorAnimation in actor.Animations)
				{
					AnimationClip animationClip = new AnimationClip();
					animationClip.name = actorAnimation.Name;
					animationClip.wrapMode = actorAnimation.IsLooping ? WrapMode.Loop : WrapMode.ClampForever;
					
					AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(animationClip);
					clipSettings.stopTime = actorAnimation.Duration;
					clipSettings.loopTime = actorAnimation.IsLooping;

					AnimationUtility.SetAnimationClipSettings(animationClip, clipSettings);

					AssetDatabase.AddObjectToAsset(animationClip, animatorController);

					EditorUtility.SetDirty(animationClip);
				}
			}

			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Assets/Nima/Make Mecanim Controller", true, 1)]
		static bool ValidateMakeMecanimController() 
		{
			foreach (object o in Selection.objects) 
			{
				if (o == null || o.GetType() != typeof(ActorAsset))
				{
					return false;
				}
			}
			return true;
		}

		public static void ReloadMecanimController(ActorBaseComponent actorComponent, UnityEditor.Animations.AnimatorController animatorController)
		{
			Nima.Actor actor = actorComponent.Asset.Actor;

			string path = AssetDatabase.GetAssetPath(animatorController);

			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
			
			List<Nima.Animation.ActorAnimation> alreadyUsedAnimations = new List<Nima.Animation.ActorAnimation>();

			foreach(UnityEngine.Object asset in assets)
			{
				AnimationClip clip = asset as AnimationClip;
				if(clip == null)
				{
					continue;
				}

				bool exists = false;
				foreach(Nima.Animation.ActorAnimation actorAnimation in actor.Animations)
				{
					if(actorAnimation.Name == clip.name)
					{
						exists = true;
						alreadyUsedAnimations.Add(actorAnimation);
						clip.SetCurve("", typeof(GameObject), "null", AnimationCurve.Linear(0, 0, actorAnimation.Duration, 0));
						AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
						clipSettings.stopTime = actorAnimation.Duration;
						clipSettings.loopTime = actorAnimation.IsLooping;
						AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

						EditorUtility.SetDirty(clip);
						break;
					}
				}
				if(!exists)
				{
					AnimationClip.DestroyImmediate(clip, true);
				}
			}

			foreach(Nima.Animation.ActorAnimation actorAnimation in actor.Animations)
			{
				int idx = alreadyUsedAnimations.IndexOf(actorAnimation);
				if(idx == -1)
				{
					AnimationClip animationClip = new AnimationClip();
					animationClip.name = actorAnimation.Name;
					animationClip.SetCurve("", typeof(GameObject), "null", AnimationCurve.Linear(0, 0, actorAnimation.Duration, 0));
					AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(animationClip);
					clipSettings.stopTime = actorAnimation.Duration;
					clipSettings.loopTime = actorAnimation.IsLooping;
					AnimationUtility.SetAnimationClipSettings(animationClip, clipSettings);

					AssetDatabase.AddObjectToAsset(animationClip, animatorController);

					EditorUtility.SetDirty(animationClip);
				}
			}

			EditorUtility.SetDirty(animatorController);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}
	}
}
