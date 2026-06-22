package com.game.render;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.VertexAttributes.Usage;
import com.badlogic.gdx.graphics.g3d.Material;
import com.badlogic.gdx.graphics.g3d.Model;
import com.badlogic.gdx.graphics.g3d.attributes.ColorAttribute;
import com.badlogic.gdx.graphics.g3d.utils.ModelBuilder;

public class ModelFactory {
    private static final long ATTRIBS = Usage.Position | Usage.Normal;

    public static Model tileModel() {
        return new ModelBuilder().createBox(0.95f, 0.15f, 0.95f,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model buildingWallsModel() {
        return new ModelBuilder().createBox(1.4f, 1.0f, 1.4f,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model buildingRoofModel() {
        return new ModelBuilder().createCone(2.0f, 0.6f, 2.0f, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model heroBodyModel() {
        return new ModelBuilder().createCapsule(0.2f, 0.5f, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model heroHeadModel() {
        return new ModelBuilder().createSphere(0.36f, 0.36f, 0.36f, 10, 10,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model heroHatModel() {
        return new ModelBuilder().createCone(0.3f, 0.35f, 0.3f, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model swordModel() {
        return new ModelBuilder().createBox(0.04f, 0.3f, 0.012f,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model shieldModel() {
        return new ModelBuilder().createBox(0.04f, 0.22f, 0.16f,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model resourceModel() {
        return new ModelBuilder().createBox(0.3f, 0.3f, 0.3f,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model enemyBodyModel() {
        return new ModelBuilder().createCapsule(0.14f, 0.35f, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model enemyHeadModel() {
        return new ModelBuilder().createSphere(0.24f, 0.24f, 0.24f, 8, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model workerBodyModel() {
        return new ModelBuilder().createCapsule(0.14f, 0.35f, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }

    public static Model workerHeadModel() {
        return new ModelBuilder().createSphere(0.22f, 0.22f, 0.22f, 8, 8,
            new Material(ColorAttribute.createDiffuse(Color.WHITE)), ATTRIBS);
    }
}
