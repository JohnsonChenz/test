using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSys
{
    public abstract partial class DialogueBaseUI
    {
        [System.Serializable]

        public class NodePool
        {
            [SerializeField, Tooltip("物件池要生成的物件")]
            public GameObject go = null;
            [SerializeField, Tooltip("物件池初始大小")]
            public int initSize = 0;
            [SerializeField, Tooltip("物件池不夠時, 是否自動新增")]
            public bool autoPut = false;
            [SerializeField, Tooltip("每次自動新增的數量")]
            public int autoPutSize = 0;
            private List<GameObject> _pool;

            private Transform _target = null;

            public NodePool()
            {
                this._pool = new List<GameObject>();
            }

            public void Init(Transform target)
            {
                this._target = target;

                for (int i = 0; i < this.initSize; i++)
                {
                    GameObject instGo = Instantiate(this.go, Vector3.zero, Quaternion.identity, this._target);
                    this.Put(instGo);
                }
            }

            private void _AutoPut()
            {
                for (int i = 0; i < this.autoPutSize; i++)
                {
                    GameObject instGo = Instantiate(this.go, Vector3.zero, Quaternion.identity, this._target);
                    this.Put(instGo);
                }
            }

            /// <summary>
            /// 物件池大小
            /// </summary>
            /// <returns></returns>
            public int Size()
            {
                return this._pool.Count;
            }

            /// <summary>
            /// 清空物件池
            /// </summary>
            public void Clear()
            {
                this._pool.ForEach(go =>
                {
                    Destroy(go);
                });
                this._pool.Clear();
            }

            /// <summary>
            /// 回收物件
            /// </summary>
            /// <param name="go"></param>
            public void Put(GameObject go)
            {
                if (this._pool.IndexOf(go) == -1 && go)
                {
                    go.transform.SetParent(this._target);
                    go.SetActive(false);
                    this._pool.Add(go);
                }
            }

            /// <summary>
            /// 取出物件
            /// </summary>
            /// <returns></returns>
            public GameObject Get()
            {
                if (this._pool.Count == 0)
                {
                    if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                    else return null;
                }

                GameObject go = this._pool.Pop();
                go.transform.SetParent(null);
                go.SetActive(true);

                return go;
            }

            /// <summary>
            /// 取出物件
            /// </summary>
            /// <returns></returns>
            public GameObject Get(Transform parent)
            {
                if (this._pool.Count == 0)
                {
                    if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                    else return null;
                }

                GameObject go = this._pool.Pop();
                go.transform.SetParent(parent);
                go.SetActive(true);

                return go;
            }

            /// <summary>
            /// 取出物件
            /// </summary>
            /// <returns></returns>
            public GameObject Get(Transform parent, Vector3 position)
            {
                if (this._pool.Count == 0)
                {
                    if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                    else return null;
                }

                GameObject go = this._pool.Pop();
                go.transform.SetParent(parent);
                go.transform.localPosition = position;
                go.SetActive(true);

                return go;
            }

            /// <summary>
            /// 取出物件
            /// </summary>
            /// <returns></returns>
            public GameObject Get(Transform parent, Vector3 position, Quaternion rotation)
            {
                if (this._pool.Count == 0)
                {
                    if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                    else return null;
                }

                GameObject go = this._pool.Pop();
                go.transform.SetParent(parent);
                go.transform.localPosition = position;
                go.transform.rotation = rotation;
                go.SetActive(true);

                return go;
            }
        }

        public class CamStudio
        {
            public Camera cam;
            public RenderTexture rt;
            public Transform root;

            public void SetCamFocus(Transform follow)
            {
                this.cam.transform.position = new Vector3(follow.position.x, follow.position.y, this.cam.transform.position.z);
            }
        }

        // --- 攝影棚設置與組件                                                                         
        public NodePool camStudioPool = new NodePool();                                                 // 攝影棚物件池
        private Dictionary<string, int> _dictCounter = new Dictionary<string, int>();                   // 計數管理
        private Dictionary<string, GameObject> _dictCamStudios = new Dictionary<string, GameObject>();  // 快取攝影棚
        private string mdRootName = "Root";                                                             // 模型定位點
        private int originSet = 20000;                                                                  // 初始位置
        private int offset = 1000;                                                                      // 攝影棚的間隔位置

        #region 攝影棚相關方法

        /// <summary>
        /// 【指定TargetRawImage自動設置】返回CamStudio, 建立一間攝影棚, 進行模型展示 (RenderTexture)
        /// </summary>
        /// <param name="targetRawImage">渲染目標</param>
        /// <param name="fov">攝影機的焦距</param>
        /// <returns>CamStudio class</returns>
        protected CamStudio CreateRtCamStudio(string mark, Transform follow, RawImage targetRawImage, float fov = 30)
        {
            CamStudio camStudio = new CamStudio();

            // 先清空rawImage的texture
            targetRawImage.texture = null;

            // 取得攝影棚數量
            int count = this._dictCamStudios.Count;

            // 計算攝影棚之間的間隔位置
            int finalPos = this.originSet + (this.offset * count);
            // 從物件池取出攝影棚, 並且設置間隔位置
            GameObject instCamStudio = this.camStudioPool.Get(this.camRoot.transform, new Vector3(finalPos, finalPos, finalPos));
            if (camStudio == null) return null;

            // 新增攝影棚至快取並且計數管理
            this._AddToCacheWithCounter(mark, instCamStudio);

            // 尋找攝影棚的攝影機
            Camera rtCam = instCamStudio.transform.GetComponentInChildren<Camera>();
            if (rtCam == null) return null;

            // 指定虛擬攝影機Field Of View (控制攝影機的焦距)
            rtCam.fieldOfView = fov;

            // 指定攝影機跟蹤點
            if(follow != null) rtCam.transform.position = new Vector3(follow.position.x, follow.position.y, rtCam.transform.position.z);

            // 產生暫存的RenderTexture
            RenderTexture tempRt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            tempRt.Create();
            tempRt.name = "TempRt";

            // 將RenderTexture指定給螢幕
            rtCam.targetTexture = tempRt;
            // 也將RenderTexture指定給TargetRawImage
            targetRawImage.texture = tempRt;

            // 尋找定位點並且返回
            Transform root = instCamStudio.transform.Find(this.mdRootName);

            camStudio.cam = rtCam;
            camStudio.rt = tempRt;
            camStudio.root = root;

            return camStudio;
        }

        /// <summary>
        /// 直接調用離開攝影棚, 建議執行於各UI的OnDisable
        /// </summary>
        protected void LeaveRtCamStudio(string mark)
        {
            if (!this._HasInCounter(mark)) return;

            int count = this._dictCounter[mark];
            for (int i = count; i >= 1; i--)
            {
                // 從快取取出攝影棚
                GameObject topCamStudio = this._dictCamStudios[mark + i.ToString()];
                // 進行相關重設與釋放
                this._ReleaseCamStudio(topCamStudio);
                // 回收攝影棚
                this.camStudioPool.Put(topCamStudio);

                // 移除攝影棚快取
                this._dictCamStudios.Remove(mark + i.ToString());
            }

            // 最後移除計數快取
            this._dictCounter.Remove(mark);
        }

        /// <summary>
        /// 回收攝影棚時, 進行相關重設與釋放
        /// </summary>
        /// <param name="camStudio"></param>
        private void _ReleaseCamStudio(GameObject camStudio)
        {
            // 將攝影機的RenderTexture設置為null
            Camera rtCam = camStudio.transform.GetComponentInChildren<Camera>();
            if (rtCam != null) rtCam.targetTexture = null;

            // 將定位點裡的模型都刪除
            Transform root = camStudio.transform.Find(this.mdRootName);
            if (root != null) root.DestroyAllChildren();
        }

        /// <summary>
        /// 加入快取並且計數
        /// </summary>
        /// <param name="mark"></param>
        /// <param name="obj"></param>
        private void _AddToCacheWithCounter(string mark, GameObject obj)
        {
            if (!this._HasInCounter(mark))
            {
                this._dictCounter.Add(mark, 1);
                this._dictCamStudios.Add(mark + 1.ToString(), obj);
            }
            else
            {
                this._dictCounter[mark]++;
                int count = this._dictCounter[mark];
                this._dictCamStudios.Add(mark + count.ToString(), obj);
            }
        }

        /// <summary>
        /// 檢查Counter快取是否已經有紀錄了
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        private bool _HasInCounter(string mark)
        {
            return this._dictCounter.ContainsKey(mark);
        }
        #endregion
    }
}
