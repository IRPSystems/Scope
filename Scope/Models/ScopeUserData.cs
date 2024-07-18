
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Scope.Models
{
	public class ScopeUserData: ObservableObject
	{

		public string ScopeRecordingDir { get; set; }



		public static ScopeUserData LoadScopeUserData(string dirName)
		{
			
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				return new ScopeUserData();
			}
			path = Path.Combine(path, "ScopeUserData.json");
			if (File.Exists(path) == false)
			{
				return new ScopeUserData();
			}

			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			ScopeUserData scopeUserData = JsonConvert.DeserializeObject(jsonString, settings) as ScopeUserData;
			if (scopeUserData == null)
				return scopeUserData;

			return scopeUserData;
		}



		public static void SaveScopeUserData(
			string dirName,
			ScopeUserData scopeUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "ScopeUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(scopeUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
