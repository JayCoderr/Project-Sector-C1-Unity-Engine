using System.Collections.Generic;

namespace MFPS.Internal.Utility
{
    public class bl_Trie
    {
        private readonly TrieNode _root;

        public bl_Trie()
        {
            _root = new TrieNode();
        }

        public void Insert(string word)
        {
            TrieNode node = _root;
            foreach (char c in word)
            {
                TrieNode nextNode;
                if (!node.Children.TryGetValue(c, out nextNode))
                {
                    nextNode = new TrieNode();
                    node.Children[c] = nextNode;
                }
                node = nextNode;
            }
            node.IsEndOfWord = true;
        }

        public bool Contains(string word)
        {
            TrieNode node = _root;
            foreach (char c in word)
            {
                if (!node.Children.TryGetValue(c, out node))
                {
                    return false;
                }
            }
            return node.IsEndOfWord;
        }

        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; }
            public bool IsEndOfWord { get; set; }

            public TrieNode()
            {
                Children = new Dictionary<char, TrieNode>();
                IsEndOfWord = false;
            }
        }
    }
}