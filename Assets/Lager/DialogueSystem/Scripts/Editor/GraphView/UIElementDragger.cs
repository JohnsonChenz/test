using UnityEngine;
using UnityEngine.UIElements;

public class UIElementDragger : MouseManipulator
{
    private Vector2 mouseStartPos; // 滑鼠點擊目標當下的座標
    protected bool active;         // 判斷啟用狀態

    /// <summary>
    /// 父階層的VisualElement
    /// </summary>
    public VisualElement parentVisualElement;

    /// <summary>
    /// 目標和父階層Layout左端(Left)之間的最大距離校正
    /// </summary>
    public float OffsetMaxDistanceFromLeft;

    /// <summary>
    /// 目標和父階層Layout頂端(Top)之間的最大距離校正
    /// </summary>
    public float OffsetMaxDistanceFromTop;

    public UIElementDragger(VisualElement target)
    {
        // 將滑鼠左鍵設定為觸發按鈕
        this.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        // 啟用判斷預設為關閉
        this.active = false;
        // 設定抓取目標
        this.target = target;
    }

    /// <summary>
    /// 註冊滑鼠點擊、拖曳、放開事件
    /// </summary>
    protected override void RegisterCallbacksOnTarget()
    {
        this.target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    /// <summary>
    /// 註銷滑鼠點擊、拖曳、放開事件
    /// </summary>
    protected override void UnregisterCallbacksFromTarget()
    {
        this.target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        this.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        this.target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }

    /// <summary>
    /// 當按下滑鼠時
    /// </summary>
    /// <param name="e">事件Handler</param>
    protected void OnMouseDown(MouseDownEvent e)
    {
        // 如果正在被拖曳
        if (this.active)
        {
            // 如果判定目標正在被拖曳，調用此方法，防止事件冒泡
            e.StopImmediatePropagation();
            return;
        }

        // 如果當前滑鼠點擊事件滿足當前啟動器的啟動條件 (假設啟動器含有滑鼠左鍵，得按下滑鼠左鍵才能滿足啟動條件)
        if (this.CanStartManipulation(e))
        {
            // 存下滑鼠座標
            this.mouseStartPos = e.localMousePosition;

            // 開啟拖曳判定
            this.active = true;

            // 使目標指派一個事件接收器去抓取滑鼠事件
            this.target.CaptureMouse();

            // 防止事件冒泡
            e.StopPropagation();
        }

    }


    /// <summary>
    /// 當移動滑鼠時
    /// </summary>
    /// <param name="e">事件Handler</param>
    protected void OnMouseMove(MouseMoveEvent e)
    {
        if (!this.active || !target.HasMouseCapture())
            return;

        // 取得拖曳滑鼠與原始滑鼠座標之兩者相差座標值
        Vector2 diff = e.localMousePosition - this.mouseStartPos;

        // 設定目標在Layout中的Top位置，鉗制參數，以防止目標被拖曳至視窗視線之外
        this.target.style.top = Mathf.Clamp(this.target.layout.y + diff.y, 0, this.parentVisualElement.layout.height - this.target.layout.height - this.OffsetMaxDistanceFromTop);
        // 設定目標在Layout中的Left位置，鉗制參數，以防止目標被拖曳至視窗視線之外
        this.target.style.left = Mathf.Clamp(this.target.layout.x + diff.x, 0, this.parentVisualElement.layout.width - this.target.layout.width - this.OffsetMaxDistanceFromLeft);

        // 防止事件冒泡
        e.StopPropagation();
    }

    /// <summary>
    /// 當放開滑鼠時
    /// </summary>
    /// <param name="e">事件Handler</param>
    protected void OnMouseUp(MouseUpEvent e)
    {
        if (!this.active || !this.target.HasMouseCapture() || !this.CanStopManipulation(e))
            return;

        // 關閉拖曳判定
        this.active = false;

        // 停止讓事件處理器抓取滑鼠
        this.target.ReleaseMouse();

        // 防止事件冒泡
        e.StopPropagation();
    }
}