using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserAlignDX.GA.BasicSpace
{
    /// <summary>
    /// 二维平面模板定位补偿计算器
    /// </summary>
    public class TemplatePositioningCalculator2D
    {
        /// <summary>
        /// 应用补偿的顺序
        /// </summary>
        public enum ApplyOrder
        {
            TranslateThenRotate,  // 先平移后旋转
            RotateThenTranslate   // 先旋转后平移
        }

        /// <summary>
        /// 二维向量结构
        /// </summary>
        public struct Vector2
        {
            public float X;
            public float Y;

            public Vector2(float x, float y)
            {
                X = x;
                Y = y;
            }

            public static Vector2 operator +(Vector2 a, Vector2 b)
            {
                return new Vector2(a.X + b.X, a.Y + b.Y);
            }

            public static Vector2 operator -(Vector2 a, Vector2 b)
            {
                return new Vector2(a.X - b.X, a.Y - b.Y);
            }

            public static Vector2 operator *(Vector2 a, float scalar)
            {
                return new Vector2(a.X * scalar, a.Y * scalar);
            }

            public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

            public override string ToString()
            {
                return $"({X:F2}, {Y:F2})";
            }
        }

        /// <summary>
        /// 模板数据
        /// </summary>
        public class TemplateData
        {
            public Vector2 Position;
            public float Angle; // 角度（度）

            public TemplateData(Vector2 pos, float ang)
            {
                Position = pos;
                Angle = ang;
            }

            public override string ToString()
            {
                return $"位置: {Position}, 角度: {Angle:F2}°";
            }
        }

        /// <summary>
        /// 补偿结果
        /// </summary>
        public class CompensationResult
        {
            public Vector2 Translation;    // 平移补偿值
            public float AngleCompensation; // 角度补偿值（度）
            public ApplyOrder Order;       // 应用顺序

            public CompensationResult(Vector2 trans, float angleComp, ApplyOrder order = ApplyOrder.TranslateThenRotate)
            {
                Translation = trans;
                AngleCompensation = angleComp;
                Order = order;
            }

            public void PrintResult()
            {
                string orderStr = Order == ApplyOrder.TranslateThenRotate ? "先平移后旋转" : "先旋转后平移";
                Console.WriteLine($"补偿结果 - 平移: {Translation}, 角度: {AngleCompensation:F2}°, 顺序: {orderStr}");
            }
            public string ResultString(float eRes)
            {
                Vector2 vec = new Vector2(Translation.X * eRes, Translation.Y * eRes);

                string orderStr = Order == ApplyOrder.TranslateThenRotate ? "先平移后旋转" : "先旋转后平移";
                string result = $"补偿结果 - 平移: {vec}, 角度: {AngleCompensation:F2}°, 顺序: {orderStr}";
                return result;
            }
        }

        /// <summary>
        /// 物体变换数据
        /// </summary>
        public class TransformData
        {
            public Vector2 Position;
            public float Rotation; // 绕Z轴旋转角度（度）

            public TransformData(Vector2 position, float rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            public override string ToString()
            {
                return $"位置: {Position}, 旋转: {Rotation:F2}°";
            }
        }

        /// <summary>
        /// 计算位置和角度补偿
        /// </summary>
        public static CompensationResult CalculateCompensation(TemplateData template, TemplateData current, ApplyOrder order = ApplyOrder.TranslateThenRotate)
        {
            // 1. 计算平移补偿（模板位置 - 当前位置）
            Vector2 translationCompensation = template.Position - current.Position;

            // 2. 计算角度差（直接换算到[-180, 180]范围）
            float angleDifference = CalculateAngleDifference(template.Angle, current.Angle);

            // 3. 如果是先旋转后平移，需要调整平移补偿（考虑旋转后的坐标系）
            if (order == ApplyOrder.RotateThenTranslate)
            {
                translationCompensation = AdjustTranslationForRotation(translationCompensation, -angleDifference);
            }

            return new CompensationResult(translationCompensation, angleDifference, order);
        }

        /// <summary>
        /// 调整平移补偿值（考虑旋转后的坐标系）
        /// </summary>
        private static Vector2 AdjustTranslationForRotation(Vector2 translation, float rotationAngle)
        {
            // 将旋转角度转换为弧度
            float rad = rotationAngle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            // 反向旋转平移向量（因为先旋转物体，所以平移需要在新的坐标系中进行）
            float newX = translation.X * cos - translation.Y * sin;
            float newY = translation.X * sin + translation.Y * cos;

            return new Vector2(newX, newY);
        }

        /// <summary>
        /// 计算两个角度之间的最小差异（考虑360度循环）
        /// 直接换算到 [-180, 180] 范围内
        /// </summary>
        private static float CalculateAngleDifference(float angle1, float angle2)
        {
            float diff = angle1 - angle2;

            // 将角度差标准化到 [-180, 180] 范围内
            while (diff > 180f)
                diff -= 360f;
            while (diff < -180f)
                diff += 360f;

            return diff;
        }

        /// <summary>
        /// 应用补偿到当前物体
        /// </summary>
        public static TransformData ApplyCompensation(TransformData currentObject, CompensationResult compensation)
        {
            Vector2 newPosition = currentObject.Position;
            float newRotation = currentObject.Rotation;

            if (compensation.Order == ApplyOrder.TranslateThenRotate)
            {
                // 先平移后旋转
                newPosition = currentObject.Position + compensation.Translation;
                newRotation = NormalizeAngle(currentObject.Rotation + compensation.AngleCompensation);
            }
            else
            {
                // 先旋转后平移
                newRotation = NormalizeAngle(currentObject.Rotation + compensation.AngleCompensation);
                newPosition = currentObject.Position + compensation.Translation;
            }

            return new TransformData(newPosition, newRotation);
        }

        /// <summary>
        /// 将角度标准化到 [0, 360) 范围
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            while (angle >= 360f) angle -= 360f;
            while (angle < 0f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// 完整的定位补偿流程
        /// </summary>
        public static (CompensationResult compensation, TransformData result) PerformCompletePositioning(
            Vector2 templatePos, float templateAngle,
            Vector2 currentPos, float currentAngle,
            ApplyOrder order = ApplyOrder.TranslateThenRotate)
        {
            TemplateData template = new TemplateData(templatePos, templateAngle);
            TemplateData current = new TemplateData(currentPos, currentAngle);

            CompensationResult compensation = CalculateCompensation(template, current, order);

            Console.WriteLine($"定位计算完成:");
            compensation.PrintResult();

            TransformData currentTransform = new TransformData(currentPos, currentAngle);
            TransformData resultTransform = ApplyCompensation(currentTransform, compensation);

            Console.WriteLine($"应用后结果: {resultTransform}");

            return (compensation, resultTransform);
        }

        /// <summary>
        /// 快速计算补偿值
        /// </summary>
        public static CompensationResult QuickCalculate(Vector2 templatePos, float templateAngle,
                                                       Vector2 currentPos, float currentAngle,
                                                       ApplyOrder order = ApplyOrder.TranslateThenRotate)
        {
            return CalculateCompensation(
                new TemplateData(templatePos, templateAngle),
                new TemplateData(currentPos, currentAngle),
                order
            );
        }
    }

    /// <summary>
    /// 测试用例和示例
    /// </summary>
    public class PositioningTest
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== 二维定位补偿测试（支持两种应用顺序）===\n");

            // 测试两种顺序的对比
            TestBothOrders("基本情况",
                new TemplatePositioningCalculator2D.Vector2(10, 5), 45f,
                new TemplatePositioningCalculator2D.Vector2(8, 6), 30f);

            TestBothOrders("有旋转的情况",
                new TemplatePositioningCalculator2D.Vector2(10, 5), 90f,
                new TemplatePositioningCalculator2D.Vector2(8, 6), 30f);

            TestBothOrders("大角度差",
                new TemplatePositioningCalculator2D.Vector2(10, 5), 10f,
                new TemplatePositioningCalculator2D.Vector2(8, 6), 200f);

            TestBothOrders("零角度",
                new TemplatePositioningCalculator2D.Vector2(10, 5), 0f,
                new TemplatePositioningCalculator2D.Vector2(8, 6), 0f);
        }

        private static void TestBothOrders(string testName,
            TemplatePositioningCalculator2D.Vector2 templatePos, float templateAng,
            TemplatePositioningCalculator2D.Vector2 currentPos, float currentAng)
        {
            Console.WriteLine($"\n--- {testName} ---");
            Console.WriteLine($"模板: 位置{templatePos}, 角度{templateAng}°");
            Console.WriteLine($"当前: 位置{currentPos}, 角度{currentAng}°");

            // 测试先平移后旋转
            Console.WriteLine($"\n[先平移后旋转]");
            var (compensation1, result1) = TemplatePositioningCalculator2D.PerformCompletePositioning(
                templatePos, templateAng, currentPos, currentAng,
                TemplatePositioningCalculator2D.ApplyOrder.TranslateThenRotate);

            // 测试先旋转后平移
            Console.WriteLine($"\n[先旋转后平移]");
            var (compensation2, result2) = TemplatePositioningCalculator2D.PerformCompletePositioning(
                templatePos, templateAng, currentPos, currentAng,
                TemplatePositioningCalculator2D.ApplyOrder.RotateThenTranslate);

            // 验证两种方法结果是否一致
            bool positionMatch = IsVectorEqual(result1.Position, result2.Position, 0.001f);
            bool angleMatch = IsAngleEqual(result1.Rotation, result2.Rotation, 0.001f);

            Console.WriteLine($"\n两种顺序对比: 位置{(positionMatch ? "一致" : "不一致")}, 角度{(angleMatch ? "一致" : "不一致")}");

            // 验证最终是否匹配模板
            bool finalPositionMatch = IsVectorEqual(result1.Position, templatePos, 0.001f);
            bool finalAngleMatch = IsAngleEqual(result1.Rotation, templateAng, 0.001f);
            Console.WriteLine($"最终验证: 位置{(finalPositionMatch ? "匹配" : "不匹配")}, 角度{(finalAngleMatch ? "匹配" : "不匹配")}");
        }

        private static bool IsVectorEqual(TemplatePositioningCalculator2D.Vector2 a,
                                        TemplatePositioningCalculator2D.Vector2 b, float tolerance)
        {
            return Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;
        }

        private static bool IsAngleEqual(float a, float b, float tolerance)
        {
            float diff = TemplatePositioningCalculator2D.NormalizeAngle(a - b);
            if (diff > 180f) diff = 360f - diff;
            return diff < tolerance;
        }
    }
}
