using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class WindowController : MonoBehaviour
{
    // Import necessary functions from the user32.dll (Windows-specific)
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, uint flags);

    // Constants for window styles
    private const int GWL_STYLE = -16;
    private const int WS_BORDER = 1;
    private const int WS_SYSMENU = 0x00080000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;

    private void Start()
    {
        // Get the window handle
        IntPtr hwnd = (IntPtr)GetWindowLong(GetDesktopWindow(), GWL_STYLE);

        // Remove the title bar and system menu (border)
        SetWindowLong(hwnd, GWL_STYLE, WS_SYSMENU);

        // Reposition the window (optional)
        SetWindowPos(hwnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
    }

    // Get the window handle (for Windows)
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();
}



