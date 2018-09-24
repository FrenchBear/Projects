// UniDataTestProject
// Unit tests for UniData
//
// 2018-09-24   PV


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniDataNS;

[TestClass]
public class UniDataUnitTests
{
    [TestMethod]
    public void LoadStaticData()
    {
        int n = UniData.CharacterRecords.Count;
        Assert.IsTrue(n > 80000);
    }


    [TestMethod]
    public void TestUnicodeLength()
    {
        string s = "Aé♫山𝄞🐗";
        Assert.AreEqual(s.Length, 8);
        Assert.AreEqual(UniData.UnicodeLength(s), 6);
    }


    [TestMethod]
    public void TestEnumCharacterRecords()
    {
        string s = "Aé♫山𝄞🐗";
        var l = UniDataNS.ExtensionMethods.EnumCharacterRecords(s).ToList();
        Assert.AreEqual(l[0], UniData.CharacterRecords[0x41]);
        Assert.AreEqual(l[1], UniData.CharacterRecords[0xE9]);
        Assert.AreEqual(l[2], UniData.CharacterRecords[0x266B]);
        Assert.AreEqual(l[3], UniData.CharacterRecords[0x5C71]);
        Assert.AreEqual(l[4], UniData.CharacterRecords[0x1D11E]);  // G CLEF
        Assert.AreEqual(l[5], UniData.CharacterRecords[0x1F417]);  // BOAR
    }

    [TestMethod]
    public void TestCharsAssignedToValidBlock()
    {
        // Check that all characters are assigned to a valid block
        foreach (var cr in UniData.CharacterRecords.Values)
            Assert.IsTrue(UniData.BlockRecords.ContainsKey(cr.BlockBegin));
    }

    [TestMethod]
    public void TestCharsAssignedToValidCategory()
    {
        // Check that all characters are assigned to a valid category
        foreach (var cr in UniData.CharacterRecords.Values)
            Assert.IsTrue(UniData.CategoryRecords.ContainsKey(cr.Category));
    }

    [TestMethod]
    public void TestNoOverlappingBlock()
    {
        HashSet<int> blocksMap = new HashSet<int>();
        foreach (var block in UniData.BlockRecords.Values)
        {
            var blockSet = new HashSet<int>(Enumerable.Range(block.Begin, block.End - block.Begin + 1));
            Assert.AreEqual(blocksMap.Intersect(blockSet).Count(), 0);
            blocksMap.UnionWith(blockSet);
        }
    }
}
