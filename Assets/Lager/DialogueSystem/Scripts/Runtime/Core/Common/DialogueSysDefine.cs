//#define ENABLE_DIALOGUE_NODE_TYPE
#define ENABLE_DIALOGUE_EXCEL_TYPE


namespace DialogueSys
{
    public static class DialogueSysDefine
    {

#if ENABLE_DIALOGUE_NODE_TYPE
        public static bool bNodeType = true;
#else
        public static bool bNodeType = false;
#endif

#if ENABLE_DIALOGUE_EXCEL_TYPE
        public static bool bExcelType = true;
#else
        public static bool bExcelType = false;
#endif
    }

    /// <summary>
    /// 講者顯示是否要反轉
    /// </summary>
    public enum DialogueSpeakerImageReverse
    {
        Reverse = -1,
        None = 1
    }
}

