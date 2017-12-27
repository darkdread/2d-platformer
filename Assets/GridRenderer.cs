using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour {

    public Material mat;


    private void OnPostRender() {
        if (!Main.EditorMode || !Editor.EnableGrid)
            return;

        if (!mat) {
            Debug.LogError("Please assign a material on the inspector!");
            return;
        }

        float offsetX = 0.5f;
        float offsetY = 0.5f;
        Vector2 startPos = new Vector2(offsetX, offsetY);
        Vector2 endPos = new Vector2(offsetX, (Game.gridHeight - 2) + offsetY);

        // Drawing the rows
        for (int x = 0; x < Game.gridWidth - 1; x++) {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            mat.SetPass(0);
            GL.Color(Color.red);
            
            GL.Vertex(startPos);
            GL.Vertex(endPos);
            startPos.x += 1;
            endPos.x += 1;

            GL.End();
            GL.PopMatrix();
        }

        
        startPos = new Vector2(offsetX, offsetY);
        endPos = new Vector2((Game.gridWidth - 2) + offsetX, offsetY);

        // Drawing the columns
        for (int y = 0; y < Game.gridHeight - 1; y++) {
            GL.Begin(GL.LINES);
            mat.SetPass(0);
            GL.Color(Color.red);
            
            GL.Vertex(startPos);
            GL.Vertex(endPos);
            startPos.y += 1;
            endPos.y += 1;

            GL.End();
        }
        
    }
}
