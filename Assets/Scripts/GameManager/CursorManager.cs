//TitleScene CursorManagerObject 
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;
[SerializeField] private Texture2D defaultCursor; // デフォルトカーソル画像
    private Vector2 hotspot = Vector2.zero;       // カーソルのクリック位置

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されない
            SetDefaultCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }
}