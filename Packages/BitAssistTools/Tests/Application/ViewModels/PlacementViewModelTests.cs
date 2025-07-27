using System;
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Application.ViewModels
{
    [TestFixture]
    public class PlacementViewModelTests
    {
        private PlacementViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _viewModel = new PlacementViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            _viewModel = null;
        }

        [Test]
        public void Constructor_InitializesAllProperties()
        {
            // Act & Assert
            Assert.IsNotNull(_viewModel.UI);
            Assert.IsNotNull(_viewModel.Duplication);
            Assert.IsNotNull(_viewModel.Rounding);
            Assert.IsNotNull(_viewModel.ObjectSwapper);
            Assert.IsNotNull(_viewModel.TransformChange);
            Assert.IsNotNull(_viewModel.Distribute);
            Assert.IsNotNull(_viewModel.Arranger);
            Assert.IsNotNull(_viewModel.Padding);
            Assert.IsNotNull(_viewModel.RandomPosition);
            Assert.IsNotNull(_viewModel.RandomRotation);
            Assert.IsNotNull(_viewModel.RandomScale);
        }

        [Test]
        public void Constructor_SetsDefaultValues()
        {
            // Assert
            Assert.AreEqual(5, _viewModel.Duplication.CopyCount);
            Assert.AreEqual(CoordinateSystem.Global, _viewModel.Duplication.Coordinate);
            Assert.AreEqual(Vector3.zero, _viewModel.Duplication.Position);
            Assert.AreEqual(Vector3.zero, _viewModel.Duplication.Rotation);
            Assert.AreEqual(Vector3.one, _viewModel.Duplication.Scale);

            Assert.AreEqual(2, _viewModel.Rounding.Multiplier);
            Assert.AreEqual(TransformFlags.All, _viewModel.Rounding.TransformFlags);

            Assert.IsNull(_viewModel.ObjectSwapper.Prefab);

            Assert.AreEqual(CoordinateSystem.Global, _viewModel.TransformChange.Coordinate);
            Assert.AreEqual(Vector3.zero, _viewModel.TransformChange.Position);
            Assert.AreEqual(Vector3.zero, _viewModel.TransformChange.Rotation);
            Assert.AreEqual(Vector3.zero, _viewModel.TransformChange.Scale);

            Assert.AreEqual(Axis.X, _viewModel.Distribute.Axis);
            Assert.AreEqual(Axis.X, _viewModel.Arranger.Axis);
            Assert.AreEqual(AlignmentType.Center, _viewModel.Arranger.AlignType);

            Assert.AreEqual(1f, _viewModel.Padding.Space);
            Assert.IsNull(_viewModel.Padding.ReferencePoint);
            Assert.AreEqual(AxisFlags.All, _viewModel.Padding.AxisFlags);

            Assert.AreEqual(1f, _viewModel.RandomPosition.Radius);
            Assert.AreEqual(AxisFlags.All, _viewModel.RandomPosition.Flags);

            Assert.AreEqual(1f, _viewModel.RandomRotation.Amount);
            Assert.AreEqual(AxisFlags.YOnly, _viewModel.RandomRotation.Flags);
            Assert.IsNotNull(_viewModel.RandomRotation.Curve);

            Assert.AreEqual(1f, _viewModel.RandomScale.Ratio);
            Assert.AreEqual(0.5f, _viewModel.RandomScale.Minimum);
            Assert.AreEqual(1.5f, _viewModel.RandomScale.Maximum);
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 0.5f), _viewModel.RandomScale.MinScale);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), _viewModel.RandomScale.MaxScale);

            Assert.AreEqual(Vector2.zero, _viewModel.ScrollPosition);
            Assert.AreEqual(8, _viewModel.SpacePixel);
        }

        [Test]
        public void UI_ExpansionState_InitializesWithDefaultValues()
        {
            // Assert
            Assert.IsFalse(_viewModel.UI.SpecialDuplicate);
            Assert.IsFalse(_viewModel.UI.Rounding);
            Assert.IsFalse(_viewModel.UI.ObjectSwapper);
            Assert.IsFalse(_viewModel.UI.SpecialTransformChange);
            Assert.IsFalse(_viewModel.UI.Distribute);
            Assert.IsFalse(_viewModel.UI.Arranger);
            Assert.IsFalse(_viewModel.UI.Padding);
            Assert.IsFalse(_viewModel.UI.RandomPosition);
            Assert.IsFalse(_viewModel.UI.RandomRotation);
            Assert.IsFalse(_viewModel.UI.RandomScale);
            Assert.IsFalse(_viewModel.UI.RoundingDetail);
            Assert.IsFalse(_viewModel.UI.PaddingReferencePoint);
            Assert.IsFalse(_viewModel.UI.RandomRotationDetail);
            Assert.IsFalse(_viewModel.UI.RandomScaleDetail);
        }

        [Test]
        public void NotifyUIStateChanged_InvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            _viewModel.OnUIStateChanged += () => eventInvoked = true;

            // Act
            _viewModel.NotifyUIStateChanged();

            // Assert
            Assert.IsTrue(eventInvoked);
        }

        [Test]
        public void OnError_EventCanBeSubscribed()
        {
            // Arrange
            string receivedMessage = null;
            _viewModel.OnError += (message) => receivedMessage = message;

            // Act - エラーメソッドを直接テストできないため、リフレクションを使用
            var method = typeof(PlacementViewModel).GetMethod("NotifyError", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_viewModel, new object[] { "Test Error" });

            // Assert
            Assert.AreEqual("Test Error", receivedMessage);
        }

        [Test]
        public void DuplicationSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.Duplication.CopyCount = 10;
            _viewModel.Duplication.Coordinate = CoordinateSystem.Local;
            _viewModel.Duplication.Position = Vector3.one;
            _viewModel.Duplication.Rotation = new Vector3(45, 90, 180);
            _viewModel.Duplication.Scale = Vector3.one * 2f;

            // Assert
            Assert.AreEqual(10, _viewModel.Duplication.CopyCount);
            Assert.AreEqual(CoordinateSystem.Local, _viewModel.Duplication.Coordinate);
            Assert.AreEqual(Vector3.one, _viewModel.Duplication.Position);
            Assert.AreEqual(new Vector3(45, 90, 180), _viewModel.Duplication.Rotation);
            Assert.AreEqual(Vector3.one * 2f, _viewModel.Duplication.Scale);
        }

        [Test]
        public void RoundingSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.Rounding.Multiplier = 5;
            _viewModel.Rounding.TransformFlags = TransformFlags.Position;

            // Assert
            Assert.AreEqual(5, _viewModel.Rounding.Multiplier);
            Assert.AreEqual(TransformFlags.Position, _viewModel.Rounding.TransformFlags);
        }

        [Test]
        public void ObjectSwapperSettings_CanBeModified()
        {
            // Arrange
            var testPrefab = new GameObject("TestPrefab");

            try
            {
                // Act
                _viewModel.ObjectSwapper.Prefab = testPrefab;

                // Assert
                Assert.AreEqual(testPrefab, _viewModel.ObjectSwapper.Prefab);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(testPrefab);
            }
        }

        [Test]
        public void TransformChangeSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.TransformChange.Coordinate = CoordinateSystem.Local;
            _viewModel.TransformChange.Position = Vector3.up;
            _viewModel.TransformChange.Rotation = Vector3.forward;
            _viewModel.TransformChange.Scale = Vector3.right;

            // Assert
            Assert.AreEqual(CoordinateSystem.Local, _viewModel.TransformChange.Coordinate);
            Assert.AreEqual(Vector3.up, _viewModel.TransformChange.Position);
            Assert.AreEqual(Vector3.forward, _viewModel.TransformChange.Rotation);
            Assert.AreEqual(Vector3.right, _viewModel.TransformChange.Scale);
        }

        [Test]
        public void DistributeSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.Distribute.Axis = Axis.Y;

            // Assert
            Assert.AreEqual(Axis.Y, _viewModel.Distribute.Axis);
        }

        [Test]
        public void ArrangerSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.Arranger.Axis = Axis.Z;
            _viewModel.Arranger.AlignType = AlignmentType.Minimum;

            // Assert
            Assert.AreEqual(Axis.Z, _viewModel.Arranger.Axis);
            Assert.AreEqual(AlignmentType.Minimum, _viewModel.Arranger.AlignType);
        }

        [Test]
        public void PaddingSettings_CanBeModified()
        {
            // Arrange
            var referenceTransform = new GameObject("Reference").transform;

            try
            {
                // Act
                _viewModel.Padding.Space = 2.5f;
                _viewModel.Padding.ReferencePoint = referenceTransform;
                _viewModel.Padding.AxisFlags = AxisFlags.XOnly;

                // Assert
                Assert.AreEqual(2.5f, _viewModel.Padding.Space);
                Assert.AreEqual(referenceTransform, _viewModel.Padding.ReferencePoint);
                Assert.AreEqual(AxisFlags.XOnly, _viewModel.Padding.AxisFlags);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(referenceTransform.gameObject);
            }
        }

        [Test]
        public void RandomPositionSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.RandomPosition.Radius = 5f;
            _viewModel.RandomPosition.Flags = AxisFlags.YOnly;

            // Assert
            Assert.AreEqual(5f, _viewModel.RandomPosition.Radius);
            Assert.AreEqual(AxisFlags.YOnly, _viewModel.RandomPosition.Flags);
        }

        [Test]
        public void RandomRotationSettings_CanBeModified()
        {
            // Arrange
            var customCurve = AnimationCurve.EaseInOut(0, 0, 1, 360);

            // Act
            _viewModel.RandomRotation.Amount = 0.5f;
            _viewModel.RandomRotation.Flags = AxisFlags.ZOnly;
            _viewModel.RandomRotation.Curve = customCurve;

            // Assert
            Assert.AreEqual(0.5f, _viewModel.RandomRotation.Amount);
            Assert.AreEqual(AxisFlags.ZOnly, _viewModel.RandomRotation.Flags);
            Assert.AreEqual(customCurve, _viewModel.RandomRotation.Curve);
        }

        [Test]
        public void RandomScaleSettings_CanBeModified()
        {
            // Arrange & Act
            _viewModel.RandomScale.Ratio = 2f;
            _viewModel.RandomScale.Minimum = 0.2f;
            _viewModel.RandomScale.Maximum = 3f;
            _viewModel.RandomScale.MinScale = Vector3.zero;
            _viewModel.RandomScale.MaxScale = Vector3.one * 5f;

            // Assert
            Assert.AreEqual(2f, _viewModel.RandomScale.Ratio);
            Assert.AreEqual(0.2f, _viewModel.RandomScale.Minimum);
            Assert.AreEqual(3f, _viewModel.RandomScale.Maximum);
            Assert.AreEqual(Vector3.zero, _viewModel.RandomScale.MinScale);
            Assert.AreEqual(Vector3.one * 5f, _viewModel.RandomScale.MaxScale);
        }

        [Test]
        public void ScrollPosition_CanBeModified()
        {
            // Arrange & Act
            _viewModel.ScrollPosition = new Vector2(100, 200);

            // Assert
            Assert.AreEqual(new Vector2(100, 200), _viewModel.ScrollPosition);
        }

        [Test]
        public void SpacePixel_CanBeModified()
        {
            // Arrange & Act
            _viewModel.SpacePixel = 16;

            // Assert
            Assert.AreEqual(16, _viewModel.SpacePixel);
        }

        [Test]
        public void UI_ExpansionState_CanBeModified()
        {
            // Arrange & Act
            _viewModel.UI.SpecialDuplicate = true;
            _viewModel.UI.Rounding = true;
            _viewModel.UI.ObjectSwapper = true;
            _viewModel.UI.SpecialTransformChange = true;
            _viewModel.UI.Distribute = true;
            _viewModel.UI.Arranger = true;
            _viewModel.UI.Padding = true;
            _viewModel.UI.RandomPosition = true;
            _viewModel.UI.RandomRotation = true;
            _viewModel.UI.RandomScale = true;
            _viewModel.UI.RoundingDetail = true;
            _viewModel.UI.PaddingReferencePoint = true;
            _viewModel.UI.RandomRotationDetail = true;
            _viewModel.UI.RandomScaleDetail = true;

            // Assert
            Assert.IsTrue(_viewModel.UI.SpecialDuplicate);
            Assert.IsTrue(_viewModel.UI.Rounding);
            Assert.IsTrue(_viewModel.UI.ObjectSwapper);
            Assert.IsTrue(_viewModel.UI.SpecialTransformChange);
            Assert.IsTrue(_viewModel.UI.Distribute);
            Assert.IsTrue(_viewModel.UI.Arranger);
            Assert.IsTrue(_viewModel.UI.Padding);
            Assert.IsTrue(_viewModel.UI.RandomPosition);
            Assert.IsTrue(_viewModel.UI.RandomRotation);
            Assert.IsTrue(_viewModel.UI.RandomScale);
            Assert.IsTrue(_viewModel.UI.RoundingDetail);
            Assert.IsTrue(_viewModel.UI.PaddingReferencePoint);
            Assert.IsTrue(_viewModel.UI.RandomRotationDetail);
            Assert.IsTrue(_viewModel.UI.RandomScaleDetail);
        }
    }
}