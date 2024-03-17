using Burmuruk.AI;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.AI
{
    [CustomEditor(typeof(PathWriter))]
    public class PathWriterEditor : Editor
    {
        public PathWriter writer;

    } 
}
