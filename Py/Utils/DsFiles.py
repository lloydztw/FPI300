"""
Module         : DsFiles
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2023/10/03
"""

import os
try:
    from .CvzUtils import *
    # from .CvzUtils import IterFilesRecursively
except:
    from CvzUtils import *
    # from CvzUtils import IterFilesRecursively
    pass


class DsFiles:
    def __init__(self, srcPath: str, ext: str, tag_index: dict = None):
        self.setPathExt(srcPath, ext, tag_index)
        pass
    def __call__(self, srcPath: str, ext: str, tag_index: dict = None):
        self.setPathExt(srcPath, ext, tag_index)
        pass
    def __iter__(self):
        for imgFile in IterFilesRecursively(self.srcPath, self.ext):
            yield imgFile
        pass

    def setPathExt(self, srcPath: str, ext: str, tag_index: dict = None):
        self.tag_index = tag_index
        self.srcPath = srcPath
        self.ext = ext
        pass
    def getTags(self, fileName: str):
        tags = fileName.split(os.path.sep)
        result = {'fname' : tags[-1]}
        if bool(self.tag_index):
            for tag, i in self.tag_index.items():
                result[tag] = tags[i]
        return result
    def items(self):
        for imgFile in IterFilesRecursively(self.srcPath, self.ext):
            tags = self.getTags(imgFile)
            # if filter is not None and not filter(tags):
            #     continue
            yield imgFile, tags
    
    @staticmethod
    def ComposeChipFileName(cate:str, shot:str, ext:str = '.jpg', chipId:int=-1, subCut:int=-1):
        """ 檔名格式:
            - [cate@] shot [#chipId] [$subCut] . ext
        """
        stem = cate + "@" + shot if cate is not None else \
               shot
        if chipId >= 0:
            stem += f"#{chipId}"
        if subCut >= 0:
            stem += f"${subCut}"
        return stem + ext
    @staticmethod
    def ParseChipFileName(fileName: str):
        """ 檔名格式:
            - [cate@] shot [#chipId] [$subCut] . ext
        """
        text = os.path.split(fileName)[-1]
        text = os.path.splitext(text)[0]
        text, subCut = text.split('$') if '$' in text else (text, -1)
        text, chipId = text.split('#') if '#' in text else (text, -1)
        cate, shot = text.split('@') if '@' in text else (None, text)
        try: chipId = int(chipId) 
        except: chipId = -1
        try: subCut = int(subCut)
        except: subCut = -1
        return (cate, shot, chipId, subCut)


if __name__ == "__main__":
    # 請使用 tests / test_DsFile.py
    pass
