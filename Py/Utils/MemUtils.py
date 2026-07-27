import ctypes

class MEMORYSTATUSEX(ctypes.Structure):
    _fields_ = [
        ("dwLength", ctypes.c_ulong),
        ("dwMemoryLoad", ctypes.c_ulong),
        ("ullTotalPhys", ctypes.c_ulonglong),
        ("ullAvailPhys", ctypes.c_ulonglong),
        ("ullTotalPageFile", ctypes.c_ulonglong),
        ("ullAvailPageFile", ctypes.c_ulonglong),
        ("ullTotalVirtual", ctypes.c_ulonglong),
        ("ullAvailVirtual", ctypes.c_ulonglong),
        ("ullAvailExtendedVirtual", ctypes.c_ulonglong),
    ]

def get_available_memory():
    stat = MEMORYSTATUSEX()
    stat.dwLength = ctypes.sizeof(stat)
    ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(stat))

    # 取得可用物理記憶體容量（以 bytes 為單位）
    available_memory = stat.ullAvailPhys

    # 將 bytes 轉換為人類可讀的格式（例如 MB 或 GB）
    available_memory_readable = round(available_memory / (1024**3),3)  # 轉換為 GB

    return available_memory_readable


if __name__ == "__main__":
    # 取得並打印可用記憶體
    available_memory = get_available_memory()
    print(f"Available Memory: {available_memory} GB")
