using DialogueSys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSys
{
    public class DialogueDataBase
    {
        private Dictionary<string, JObject> _dictDialogueDataBase;

        public DialogueDataBase()
        {
            this._dictDialogueDataBase = new Dictionary<string, JObject>();
        }

        private bool _LoadDialogueDataBaseIntoCache(string dbPath)
        {
            TextAsset dbTxt = Resources.Load<TextAsset>(dbPath);

            if (dbTxt == null)
            {
                Debug.Log(string.Format("<color=#FF0000>查找不到此路徑 Json檔 【{0}】，將不被加載 </color>", dbPath));
                return false;
            }

            JObject jObj = null;
            try
            {
                jObj = JsonConvert.DeserializeObject<JObject>(dbTxt.ToString());
            }
            catch
            {
                Debug.Log(string.Format("<color=#FF0000>解析DB資料錯誤，將不被加載</color>", dbPath));
            }

            if (jObj == null) return false;

            if (jObj.Property("export_type") != null)
            {
                jObj = jObj.SelectToken<JObject>("data");
            }

            this._dictDialogueDataBase.Add(dbPath, jObj);

            return this._HasInCache(dbPath);
        }

        private bool _HasInCache(string dbPath)
        {
            return this._dictDialogueDataBase.ContainsKey(dbPath);
        }

        private void _Release(string dbPath)
        {
            this._dictDialogueDataBase.Remove(dbPath);
        }


        public void ReloadDialogueDataBase(string dbPath)
        {
            if (this._HasInCache(dbPath))
            {
                this._Release(dbPath);
                this._LoadDialogueDataBaseIntoCache(dbPath);
            }
        }

        public JObject GetDataBase(string dbPath)
        {
            if (this._HasInCache(dbPath) || this._LoadDialogueDataBaseIntoCache(dbPath)) return this._dictDialogueDataBase[dbPath];

            Debug.Log(string.Format("<color=#FF0000>無法取得Json資料!!</color>", dbPath));
            return null;
        }
    }
}
