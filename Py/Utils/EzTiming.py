from datetime import datetime
import os

OPT_TRACE = False


class EzTiming:
    def __init__(self, csharp_tag = None):
        if OPT_TRACE:
            self.dump_compare(csharp_tag)
            self.reset()
        pass
    def reset(self):
        if OPT_TRACE:
            self.tm0 = datetime.now()
            self.marks = []
        pass
    def mark(self, tag):
        if OPT_TRACE:
            ts = datetime.now() - self.tm0
            self.marks.append((tag, ts.total_seconds()))
            self.tm0 = datetime.now()
        pass
    def dump(self):
        if OPT_TRACE:
            total_secs = 0
            for mark in self.marks:
                tag, dt = mark
                total_secs += dt
                print(tag, "=", f"{dt*1000 : 0.2f} ms")
            print("total", "=", f"{total_secs*1000 : 0.2f} ms")
            print("Final Tag", "=", datetime.now().strftime("%H:%M:%S.%f")[:-3])
        pass
    def dump_compare(self, csharp_tag):
        if OPT_TRACE:        
            if csharp_tag is not None:
                tmNow = datetime.now()
                print("C#", os.path.splitext(csharp_tag.replace('_',':'))[0])
                print("Py", tmNow.strftime("%H:%M:%S.%f")[:-3])
        pass



