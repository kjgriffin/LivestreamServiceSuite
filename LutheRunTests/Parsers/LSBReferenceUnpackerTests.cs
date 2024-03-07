using Microsoft.VisualStudio.TestTools.UnitTesting;
using LutheRun.Parsers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LutheRun.Parsers.LSBReferenceUnpacker;
using CCUI_UI;
using Moq;
using AngleSharp.Text;
using Concord;

namespace LutheRun.Parsers.Tests
{
    [TestClass()]
    public class LSBReferenceUnpackerTests
    {

        #region ParseSections
        [TestMethod()]
        public void ParseSections_SimpleBook_FullChapter()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("Psalm 67");

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res.First().Book == "Psalm");
            Assert.IsTrue(res.First().StartChapter == 67);
            Assert.IsTrue(res.First().StartChapter == 67);
            Assert.IsTrue(res.First().StartVerse == 1);
            Assert.IsTrue(res.First().EndVerse == -1);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_SingleVerse()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("Matthew 15:21");

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res.First().Book == "Matthew");
            Assert.IsTrue(res.First().StartChapter == 15);
            Assert.IsTrue(res.First().EndChapter == 15);
            Assert.IsTrue(res.First().StartVerse == 21);
            Assert.IsTrue(res.First().EndVerse == 21);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_MultiVerse()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("Matthew 15:21-22");

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res.First().Book == "Matthew");
            Assert.IsTrue(res.First().StartChapter == 15);
            Assert.IsTrue(res.First().EndChapter == 15);
            Assert.IsTrue(res.First().StartVerse == 21);
            Assert.IsTrue(res.First().EndVerse == 22);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_MultiVerseCollection()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("Matthew 15:1, 4-5");

            Assert.IsTrue(res.Count == 2);
            Assert.IsTrue(res.First().Book == "Matthew");
            Assert.IsTrue(res.First().StartChapter == 15);
            Assert.IsTrue(res.First().EndChapter == 15);
            Assert.IsTrue(res.First().StartVerse == 1);
            Assert.IsTrue(res.First().EndVerse == 1);
            Assert.IsTrue(res.Last().Book == "Matthew");
            Assert.IsTrue(res.Last().StartChapter == 15);
            Assert.IsTrue(res.Last().EndChapter == 15);
            Assert.IsTrue(res.Last().StartVerse == 4);
            Assert.IsTrue(res.Last().EndVerse == 5);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_SingleSection_ChapterSplit()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("1 John 15:1-16:6");

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res.First().Book == "1 John");
            Assert.IsTrue(res.First().StartChapter == 15);
            Assert.IsTrue(res.First().EndChapter == 16);
            Assert.IsTrue(res.First().StartVerse == 1);
            Assert.IsTrue(res.First().EndVerse == 6);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_MultiSection_Multi()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("Romans 11:1-2a, 13–15, 28–32");

            Assert.IsTrue(res.Count == 3);
            Assert.IsTrue(res[0].Book == "Romans");
            Assert.IsTrue(res[0].StartChapter == 11);
            Assert.IsTrue(res[0].EndChapter == 11);
            Assert.IsTrue(res[0].StartVerse == 1);
            Assert.IsTrue(res[0].EndVerse == 2);
            Assert.IsTrue(res[1].Book == "Romans");
            Assert.IsTrue(res[1].StartChapter == 11);
            Assert.IsTrue(res[1].EndChapter == 11);
            Assert.IsTrue(res[1].StartVerse == 13);
            Assert.IsTrue(res[1].EndVerse == 15);
            Assert.IsTrue(res[2].Book == "Romans");
            Assert.IsTrue(res[2].StartChapter == 11);
            Assert.IsTrue(res[2].EndChapter == 11);
            Assert.IsTrue(res[2].StartVerse == 28);
            Assert.IsTrue(res[2].EndVerse == 32);
        }

        [TestMethod()]
        public void ParseSections_SimpleBook_MultiSection_MultiChapter()
        {
            var test = new LSBReferenceUnpacker();
            var res = test.ParseSections("2 Corinthians 3:12–13; 4:1–6");

            Assert.IsTrue(res.Count == 2);
            Assert.IsTrue(res[0].Book == "2 Corinthians");
            Assert.IsTrue(res[0].StartChapter == 3);
            Assert.IsTrue(res[0].EndChapter == 3);
            Assert.IsTrue(res[0].StartVerse == 12);
            Assert.IsTrue(res[0].EndVerse == 13);
            Assert.IsTrue(res[1].Book == "2 Corinthians");
            Assert.IsTrue(res[1].StartChapter == 4);
            Assert.IsTrue(res[1].EndChapter == 4);
            Assert.IsTrue(res[1].StartVerse == 1);
            Assert.IsTrue(res[1].EndVerse == 6);
        }


        #endregion

        #region EnumerateVerses

        [TestMethod()]
        public void EnumerateVerses_SingleVerse()
        {
            var test = new LSBReferenceUnpacker();

            var section = new SectionReference
            {
                Book = "Genesis",
                StartChapter = 1,
                EndChapter = 1,
                StartVerse = 1,
                EndVerse = 1,
            };
            var info = new Mock<IBibleInfoProvider>();
            info.Setup(x => x.GetChapterCount("Genesis")).Returns(50);
            info.Setup(x => x.GetVerseCount("Genesis", 1)).Returns(31);

            var res = test.EnumerateVerses(section, info.Object);

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res[0].Book == "Genesis");
            Assert.IsTrue(res[0].Chapter == 1);
            Assert.IsTrue(res[0].Verse == 1);
        }

        [TestMethod()]
        public void EnumerateVerses_SingleChapter_VerseSpan()
        {
            var test = new LSBReferenceUnpacker();

            var section = new SectionReference
            {
                Book = "Genesis",
                StartChapter = 1,
                EndChapter = 1,
                StartVerse = 5,
                EndVerse = 7,
            };
            var info = new Mock<IBibleInfoProvider>();
            info.Setup(x => x.GetChapterCount("Genesis")).Returns(50);
            info.Setup(x => x.GetVerseCount("Genesis", 1)).Returns(31);

            var res = test.EnumerateVerses(section, info.Object);

            Assert.IsTrue(res.Count == 3);
            Assert.IsTrue(res[0].Book == "Genesis");
            Assert.IsTrue(res[0].Chapter == 1);
            Assert.IsTrue(res[0].Verse == 5);
            Assert.IsTrue(res[1].Book == "Genesis");
            Assert.IsTrue(res[1].Chapter == 1);
            Assert.IsTrue(res[1].Verse == 6);
            Assert.IsTrue(res[2].Book == "Genesis");
            Assert.IsTrue(res[2].Chapter == 1);
            Assert.IsTrue(res[2].Verse == 7);
        }

        [TestMethod()]
        public void EnumerateVerses_SingleChapter_WholeChapter()
        {
            var test = new LSBReferenceUnpacker();

            var section = new SectionReference
            {
                Book = "Test",
                StartChapter = 5,
                EndChapter = 5,
                StartVerse = 1,
                EndVerse = -1,
            };
            var info = new Mock<IBibleInfoProvider>();
            info.Setup(x => x.GetChapterCount("Test")).Returns(12);
            info.Setup(x => x.GetVerseCount("Test", 5)).Returns(5);

            var res = test.EnumerateVerses(section, info.Object);

            Assert.IsTrue(res.Count == 5);
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(res[i].Book == "Test");
                Assert.IsTrue(res[i].Chapter == 5);
                Assert.IsTrue(res[i].Verse == i + 1);
            }
        }

        #endregion

    }
}