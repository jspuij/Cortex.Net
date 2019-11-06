// <copyright file="Tree.cs" company="Michel Weststrate, Jan-Willem Spuij">
// Copyright 2019 Michel Weststrate, Jan-Willem Spuij
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
// the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace Cortex.Net.Api
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Tree class.
    /// </summary>
    public class TreeNode
    {
        private readonly List<TreeNode> children = new List<TreeNode>();

        /// <summary>
        /// Gets name of this item.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets children of this TreeNode.
        /// </summary>
        public IEnumerable<TreeNode> Children => this.children;

        /// <summary>
        /// Creates the dependency tree from this node to all the children it is observing.
        /// </summary>
        /// <param name="startNode">The node to start with.</param>
        /// <returns>A complete tree from the current node.</returns>
        public static TreeNode GetDependencyTree(IDependencyNode startNode)
        {
            var stack = new Stack<(IDependencyNode, TreeNode)>();
            stack.Push((startNode, null));

            TreeNode result = null;

            while (stack.Count > 0)
            {
                (IDependencyNode node, TreeNode parent) = stack.Pop();
                var treeNode = new TreeNode()
                {
                    Name = node.Name,
                };

                if (parent != null)
                {
                    parent.children.Add(treeNode);
                }

                if (result == null)
                {
                    result = treeNode;
                }

                if (node is IDerivation derivation)
                {
                    foreach (var childNode in derivation.Observing.Distinct().Reverse())
                    {
                        stack.Push((childNode, treeNode));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an observer tree from this node to all the parents that observe it.
        /// </summary>
        /// <param name="startNode">The node to start with.</param>
        /// <returns>A complete tree from the current node.</returns>
        public static TreeNode GetObserverTree(IDependencyNode startNode)
        {
            var stack = new Stack<(IDependencyNode, TreeNode)>();
            stack.Push((startNode, null));

            TreeNode result = null;

            while (stack.Count > 0)
            {
                (IDependencyNode node, TreeNode parent) = stack.Pop();
                var treeNode = new TreeNode()
                {
                    Name = node.Name,
                };

                if (parent != null)
                {
                    parent.children.Add(treeNode);
                }

                if (result == null)
                {
                    result = treeNode;
                }

                if (node is IObservable observable)
                {
                    foreach (var childNode in observable.Observers.Distinct().Reverse())
                    {
                        stack.Push((childNode, treeNode));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Prints the current tree.
        /// </summary>
        /// <returns>A string with line breaks and indentation.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            Stack<(TreeNode, int)> stack = new Stack<(TreeNode, int)>();
            stack.Push((this, 0));

            while (stack.Count > 0)
            {
                (TreeNode node, int level) = stack.Pop();
                stringBuilder.AppendLine($"{string.Join(string.Empty, Enumerable.Repeat("\t", level))}{node.Name}");
                foreach (var child in node.Children.Reverse())
                {
                    stack.Push((child, level + 1));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
