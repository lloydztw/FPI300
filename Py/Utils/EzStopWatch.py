from datetime import datetime

class EzStopWatch:
    def __init__(self) -> None:
        self.reset()
    
    def reset(self):
        self.min_ms = 10**9
        self.max_ms = 0
        self.acc_ms = 0
        self.acc_cnt = 0
        self.tm0 = datetime.now()

    def start(self):
        self.tm0 = datetime.now()

    def stop(self):
        ts = datetime.now() - self.tm0
        ms = ts.total_seconds() * 1000
        self.min_ms = min(self.min_ms, ms)
        self.max_ms = max(self.max_ms, ms)
        self.acc_ms += ms
        self.acc_cnt += 1        
        return ms

    def dump(self, tag = None, stop = False):
        if stop:
            self.stop()
        acc_cnt = max(self.acc_cnt,1)
        if acc_cnt > 1:
            if tag is not None:
                print(f"[{tag}]")
            print(f"min = {self.min_ms:0.3f} ms")
            print(f"max = {self.max_ms:0.3f} ms")
            print(f"ave = {self.acc_ms/acc_cnt:0.3f} ms")
        else:
            if tag is None:
                tag = "time"
            print(f"[{tag}] = {self.acc_ms/acc_cnt:0.3f} ms")        
        pass
    