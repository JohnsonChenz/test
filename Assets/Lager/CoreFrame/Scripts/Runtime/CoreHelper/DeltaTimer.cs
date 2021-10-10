using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaTimer
{
    private bool _playing;
    private float _accTime;
    private float _intervalTime;
    private float _pauseTime;
    private float _deltaTime;

    private float _triggerTime;

    private float _tick = 0.0f;
    private float _lastTickTime;

    private float _mark;
    private float _speed;

    public DeltaTimer()
    {
        this.Reset();
    }

    public DeltaTimer(bool isPlay)
    {
        this.Reset();
        if (isPlay) this.Play();
    }

    /// <summary>
    /// 需要透過Update呼叫驅動Timer
    /// </summary>
    /// <param name="dt"></param>
    public void UpdateTimer(float dt)
    {
        this._deltaTime = dt;
        this._accTime += this._deltaTime;
    }

    public void Reset()
    {
        this._playing = false;
        this._accTime = 0.0f;
        this._intervalTime = 0.0f;
        this._pauseTime = 0.0f;
        this._deltaTime = 0.0f;
        this._triggerTime = 0.0f;
        this._tick = 0.0f;
        this._lastTickTime = 0.0f;
        this._mark = 0.0f;
        this._speed = 1.0f;
    }

    public float GetTime()
    {
        if (!this._playing) return this._pauseTime - this._intervalTime;
        return (this._accTime - this._intervalTime) * this._speed;
    }

    public float GetDeltaTime()
    {
        return this._deltaTime;
    }

    public void Pause()
    {
        if (!this._playing) return;
        this._pauseTime = this._accTime;
        this._playing = false;
    }

    public void Play()
    {
        this._intervalTime += this._accTime - this._pauseTime;
        this._playing = true;
    }

    public bool IsPause()
    {
        return !this._playing;
    }

    public bool IsPlaying()
    {
        return this._playing;
    }

    #region Timer, 依照設置的時間下去計時
    /// <summary>
    /// 設置要計時的秒數
    /// </summary>
    /// <param name="time"></param>
    public void SetTimer(float time)
    {
        this._triggerTime = this.GetTime() + time;
    }

    /// <summary>
    /// 計算觸發時間倒數計時, 如果超過設置的觸發時間將直接返回0
    /// </summary>
    /// <returns></returns>
    public float TimerCountdown()
    {
        float tempTime = this.GetTime();

        if (tempTime >= this._triggerTime) return 0.0f;
        return this._triggerTime - tempTime;
    }

    /// <summary>
    /// 返回計時時間是否已經到了
    /// </summary>
    /// <returns></returns>
    public bool IsTimerTimeout()
    {
        if (this.GetTime() < this._triggerTime) return false;
        return true;
    }
    #endregion

    #region Tick, 持續依照Set的時間Tick
    /// <summary>
    /// 設置Tick時間, 當TickTimeout時還會持續循環Tick
    /// </summary>
    /// <param name="tick"></param>
    public void SetTick(float tick)
    {
        this._tick = tick;
        this._lastTickTime = this.GetTime() + tick;
    }

    /// <summary>
    /// 取得設置的Tick的時間
    /// </summary>
    /// <returns></returns>
    public float GetTick()
    {
        return this._tick;
    }

    /// <summary>
    /// Tick觸發時間倒數, 如果超過設置的觸發時間將直接返回0
    /// </summary>
    /// <returns></returns>
    public float TickCountdown()
    {
        float tempTime = this.GetTime();

        if (tempTime >= this._lastTickTime) return 0.0f;
        return this._lastTickTime - tempTime;
    }

    /// <summary>
    /// 返回Tick時間是否已經到了
    /// </summary>
    /// <returns></returns>
    public bool IsTickTimeout()
    {
        float tempTime = this.GetTime();

        if (tempTime < this._lastTickTime) return false;
        this._lastTickTime = tempTime + _tick;
        return true;
    }
    #endregion

    #region Mark, 標記時間
    /// <summary>
    /// 設置標記時間
    /// </summary>
    public void SetMark()
    {
        this._mark = this.GetTime();
    }

    /// <summary>
    /// 取得標記時間
    /// </summary>
    /// <returns></returns>
    public float GetMark()
    {
        return this._mark;
    }

    /// <summary>
    /// 取得上次標記時的經過時間
    /// </summary>
    /// <returns></returns>
    public float GetElapsedMarkTime()
    {
        float time = this.GetTime();

        if (time == this._mark || time < this._mark) return 0.0f;

        return (time - this._mark);
    }
    #endregion

    /// <summary>
    /// 設置時間運轉速度
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        this._speed = speed;
    }

    public float GetSpeed()
    {
        return this._speed;
    }
}
