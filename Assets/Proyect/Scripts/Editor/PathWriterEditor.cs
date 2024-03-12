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

        public override void OnInspectorGUI()
        {
            writer = (PathWriter)target;

            DrawDefaultInspector();

            GUILayout.Space(10);

            string btnReadConnections = "Save connections";

            if (writer.nodesList != null)
            {
                if (GUILayout.Button(btnReadConnections))
                {
                    writer.SaveConnections();
                }
            }
        }
    } 
}
