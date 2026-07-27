"""
Module         : DsAllDataFiles
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2024/11/07
"""

import os
import pandas as pd
from .DsFiles import *


class DsFilesDataframe:
    def __init__(self, dsSrc: DsFiles = None) -> None:
        self._cacheDf: pd.DataFrame = None
        self._srcPath = None
        self._ext = None
        if dsSrc is not None:
            self.ImportFiles(dsSrc)
        pass
    
    def __iter__(self):
        for imgFile in self.IterFullPathFileNames():
            yield imgFile
        pass

    def __csv_file(self):
        return os.path.join(self._srcPath, "_summary.csv")
    
    @property
    def DF(self):
        return self._cacheDf

    def ImportFiles(self, dsSrc: DsFiles):
        dataList = []
        subFolder = ''
        srcPath = dsSrc.srcPath
        for imgFile, tags in dsSrc.items():
            record = [tags[key] for key in tags]
            fname = tags['fname']
            subFolder = imgFile.replace(srcPath,'').replace(fname,'').strip('\\')
            record.append(subFolder)
            dataList.append(record)
        if len(dataList) > 0:
            cols = [key for key in tags.keys()]
            cols.append('subFolder')
            df = pd.DataFrame(dataList, columns=cols)
            self._srcPath = srcPath
            self._ext = dsSrc.ext
            self._cacheDf = df
            return df
        None

    def LoadDataFrame(self):
        file = self.__csv_file()
        if os.path.isfile(file):
            existDf = pd.read_csv(file)
            if len(existDf) > 0 and 'fname' in existDf.columns:
                self._cacheDf = existDf
                return existDf
        self._cacheDf = None
        return None
    
    def SaveDataFrame(self):
        df = self._cacheDf
        if df is not None:
            df.to_csv(self.__csv_file(), index=False)
        pass

    def GetPath(self, dset):
        subFolder = dset['subFolder']
        if len(subFolder) > 0:
            path = os.path.join(self._srcPath, subFolder)
        else:
            path = self._srcPath
        return path
    
    def GetFullFileName(self, dset):
        return os.path.join(self.GetPath(dset), dset.fname)

    def IterFnames(self):
        df = self.LoadDataFrame()
        for rid, dset in df.iterrows():
            yield dset.fname

    def IterPathAndFileNames(self):
        df = self._cacheDf
        for _, dset in df.iterrows():
            path = self.GetPath(dset)
            # subFolder = dset['subFolder']
            # if len(subFolder) > 0:
            #     path = os.path.join(self._srcPath, subFolder)
            # else:
            #     path = self._srcPath
            yield path, dset.fname

    def IterFullPathFileNames(self):
        df = self._cacheDf
        for _, dset in df.iterrows():
            yield self.GetFullFileName(dset)
        