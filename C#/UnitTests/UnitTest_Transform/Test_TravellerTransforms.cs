using JetEazy.Match;
using JetEazy.QMath;
using LaserAlignDX.Model.Coords;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace LaserAlignDX.Tests
{
    [TestClass]
    public class TravellerTransformsTests
    {
        private const string TEST_NAME = "UnitTest_Migration_Target";

        [TestInitialize]
        public void Setup()
        {
            // 測試前清理，確保環境乾淨
            TravellerTransformFactory.DisposeAll();
        }

        [TestMethod]
        public void Test_LinearMigration_Accuracy()
        {
            // 1. 準備模擬數據
            // 假設基準 Pitch 是 10.0, 10.0
            // 新產品的 Pitch 是 12.5, 12.5
            var newPitch = new QVector(12.5, 12.5);
            int rows = 3;
            int cols = 3;

            // 模擬相機看到的格點 (EzBlocsGrid)
            // 假設相機 1 pixel = 0.1mm, 且無旋轉
            // 則 Pitch 12.5mm 對應到相機上應該是 125 pixels
            var mockCamGrid = new MockEzBlocsGrid(rows, cols, 125.0, 125.0);

            // 2. 執行線性遷移
            ITravellerTransforms migratedTrf = TravellerTransformFactory.CreateLinearMigration(
                TEST_NAME,
                CarrierEnum.C1,
                mockCamGrid,
                newPitch
            );

            // 3. 驗證遷移結果
            Assert.IsNotNull(migratedTrf, "遷移後的物件不應為 null");

            // 驗證 World Grid 是否更新
            var worldGrid = migratedTrf.GetWorldGridPoints();
            Assert.AreEqual(12.5, worldGrid.PitchX, 0.0001, "PitchX 未正確更新");

            // 4. 驗證座標轉換精度 (關鍵測試)
            // 測試目標：輸入相機中心點 (假設在 index r=1, c=1)
            // 預期輸出：物理座標應為 (12.5, 12.5)
            var camCenterPixel = mockCamGrid.Get(1, 1).Center; // (125, 125)

            var transCP = migratedTrf.GetCameraPhysicTransform(CarrierEnum.C1);
            var resultWorld = transCP.Trans(camCenterPixel);

            // 驗證轉換後的物理座標是否符合預期
            double expectedX = 12.5;
            double expectedY = 12.5;

            Assert.AreEqual(expectedX, resultWorld.X, 0.01, "相機轉物理 X 座標誤差過大");
            Assert.AreEqual(expectedY, resultWorld.Y, 0.01, "相機轉物理 Y 座標誤差過大");
        }

        [TestMethod]
        public void Test_Factory_Instance_Persistence()
        {
            // 測試 Factory 是否正確快取實例
            var trf1 = TravellerTransformFactory.Instance("SampleA");
            var trf2 = TravellerTransformFactory.Instance("SampleA");

            Assert.AreSame(trf1, trf2, "Factory 應針對相同名稱回傳同一個實例");
        }
    }

    #region Mock Objects for Testing
    // 模擬 EzBlocsGrid 的行為，用於單元測試
    public class MockEzBlocsGrid : EzBlocsGrid
    {
        public MockEzBlocsGrid(int r, int c, double px, double py)
        {
            this.Rows = r;
            this.Cols = c;
            // 填充模擬點位
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    this.Set(i, j, new MockNode(j * px, i * py));
        }
    }

    public class MockNode
    {
        public QVector Center { get; set; }
        public MockNode(double x, double y) { Center = new QVector(x, y); }
        public bool IsMajorNode() => true;
    }
    #endregion
}

