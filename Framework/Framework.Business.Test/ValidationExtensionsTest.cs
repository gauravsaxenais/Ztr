namespace ZTR.Framework.Business.Test
{
    using System.Linq;
    using EnsureThat;
    using Xunit;
    using ZTR.Framework.Business;

    public sealed class ValidationExtensionsTest : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public ValidationExtensionsTest(TestFixture testFixture)
        {
            EnsureArg.IsNotNull(testFixture, nameof(testFixture));

            _testFixture = testFixture;
        }

        [Fact]
        public void ToIndexedItems()
        {
            const long firstId = 2;
            const long secondId = 53;

            const int positionOfFirst = 38;
            const int positionOfSecond = 15;

            var idList = new[] { firstId, secondId, 22, 13, 88, 5 };
            var widgets = new WidgetFaker<WidgetUpdateModel>().Generate(100);

            widgets[positionOfFirst].Id = firstId;
            widgets[positionOfSecond].Id = secondId;

            var results = widgets.ToIndexedItems(idList, x => x.Id).ToList();

            Assert.Equal(2, results.Count());
            Assert.True(results[0].Item.Id == firstId);
            Assert.True(results[1].Item.Id == secondId);
        }

        [Fact]
        public void ToGroupedItems()
        {
            const int count = 7;
            const int countMax = 100;
            const long key = 432;
            var widgets = new WidgetFaker<WidgetCreateModel>().Generate(countMax);

            for (int i = 0; i < count; i++)
            {
                var index = 4 * i;
                widgets[index].ForeignKeyId = key;
            }

            var results = widgets.ToGroupedItems(x => x.ForeignKeyId).ToList();
            var justTheKeyedItems = results.SelectMany(x => x.Items.Where(y => y.ForeignKeyId == key)).ToList();

            Assert.Equal(countMax + 1 - count, results.Count);
            Assert.Equal(count, justTheKeyedItems.Count());
        }

        [Fact]
        public void ToGroupedItemsWithMultiPartKey()
        {
            const int count = 7;
            const int countMax = 100;
            const int key1 = 6;
            const string key2 = "MultiPartCodeTest";
            var widgets = new WidgetFaker<WidgetUpdateModel>().Generate(countMax);

            for (int i = 0; i < count; i++)
            {
                var index = 3 * i;
                widgets[index].Id = key1;
                widgets[index].Code = key2;
            }

            var results = widgets.ToGroupedItems(x => new { x.Id, x.Code }).ToList();
            var justTheKeyedItems = results.SelectMany(x => x.Items.Where(y => y.Id == key1 && y.Code == key2)).ToList();

            Assert.Equal(countMax + 1 - count, results.Count);
            Assert.Equal(count, justTheKeyedItems.Count());
        }

        [Fact]
        public void FindDuplicatesWithSingleKey()
        {
            const int count = 3;
            const int countMax = 100;
            const string code = "YUPYUP";
            const int indexFactor = 6;

            var widgets = new WidgetFaker<WidgetCreateModel>().Generate(countMax).ToIndexedItems().ToList();

            for (int i = 0; i < count; i++)
            {
                var index = indexFactor * i;
                var widget = widgets[index];

                widget.Item.Code = code;
            }

            var duplicates = widgets.FindDuplicates(x => x.Item.Code).ToList();

            Assert.Equal(count - 1, duplicates.Count);
            Assert.True(duplicates.TrueForAll(x => x.OrdinalPosition % indexFactor == 0));
        }

        [Fact]
        public void FindDuplicatesWithMultiplePartKey()
        {
            const int count = 4;
            const int countMax = 100;
            const string code = "Da_Code";
            const int foreignKeyId = 4534;
            const int indexFactor = 10;

            var widgets = new WidgetFaker<WidgetCreateModel>().Generate(countMax);

            for (int temp = 0; temp < count; temp++)
            {
                var index = indexFactor * temp;
                var widget = widgets[index];

                widget.Code = code;
                widget.ForeignKeyId = foreignKeyId;
            }

            var duplicates = widgets.ToIndexedItems().FindDuplicates(x => new { x.Item.Code, x.Item.ForeignKeyId }).ToList();

            Assert.Equal(count - 1, duplicates.Count);
            Assert.True(duplicates.TrueForAll(x => x.OrdinalPosition % indexFactor == 0));
        }
    }
}
