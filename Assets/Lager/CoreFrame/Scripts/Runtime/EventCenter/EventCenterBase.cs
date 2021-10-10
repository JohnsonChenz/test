using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventSys
{
    public class EventCenterBase
    {
        public const int EVENT_xBASE = 0x0000;

        private Dictionary<int, EventBase> _dictEvents = new Dictionary<int, EventBase>();

        /// <summary>
        /// 直接呼叫事件, 不帶入參數
        /// </summary>
        /// <param name="funcId"></param>
        public void DirectCall(int funcId)
        {
            EventBase eventBase = this._GetFromCached(funcId);
            if (eventBase != null) eventBase.HandleEvent().Forget();
        }

        /// <summary>
        /// 透過funcId取得事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcId"></param>
        /// <returns></returns>
        public T GetEvent<T>(int funcId) where T : EventBase
        {
            return (T)this._GetFromCached(funcId);
        }

        /// <summary>
        /// 透過funcId註冊事件
        /// </summary>
        /// <param name="eventBase"></param>
        protected void _Register(EventBase eventBase)
        {
            if (this._HasInCahced(eventBase.GetFuncId()))
            {
                Debug.Log(string.Format("<color=#FF0000>Repeat registration. Event funcId: {0}</color>", eventBase.GetFuncId()));
                return;
            }

            this._dictEvents.Add(eventBase.GetFuncId(), eventBase);
        }

        private EventBase _GetFromCached(int funcId)
        {
            if (!this._HasInCahced(funcId))
            {
                Debug.Log(string.Format("<color=#FF0000>Cannot found Event. Event Id: {0}</color>", funcId));
                return null;
            }

            return this._dictEvents[funcId];
        }

        private bool _HasInCahced(int funcId)
        {
            return this._dictEvents.ContainsKey(funcId);
        }
    }
}