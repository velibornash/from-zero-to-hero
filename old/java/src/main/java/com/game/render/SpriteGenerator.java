package com.game.render;

import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.Texture.TextureFilter;

public class SpriteGenerator {

    private static Texture make(Pixmap p) {
        Texture t = new Texture(p);
        t.setFilter(TextureFilter.Linear, TextureFilter.Linear);
        p.dispose();
        return t;
    }

    private static void drawEllipse(Pixmap p, int cx, int cy, int rx, int ry, float r, float g, float b, float a) {
        p.setColor(r, g, b, a);
        for (int x = -rx; x <= rx; x++) {
            int h = (int) Math.sqrt(ry * ry * (1 - (x * x) / (double) (rx * rx)));
            for (int y = -h; y <= h; y++) {
                p.drawPixel(cx + x, cy + y);
            }
        }
    }

    private static void outline(Pixmap p, int x, int y, int w, int h, float r, float g, float b) {
        p.setColor(r, g, b, 0.8f);
        p.drawRectangle(x, y, w, h);
    }

    private static void fillDiamond(Pixmap p, float r, float g, float b, float a) {
        p.setColor(r, g, b, a);
        int cx = 32, cy = 16;
        p.fillTriangle(cx, cy, 64, 16, 32, 0);
        p.fillTriangle(cx, cy, 64, 16, 32, 32);
        p.fillTriangle(cx, cy, 0, 16, 32, 32);
        p.fillTriangle(cx, cy, 0, 16, 32, 0);
    }

    public static Texture heroTexture() {
        Pixmap p = new Pixmap(64, 64, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // shadow
        p.setColor(0, 0, 0, 0.15f);
        drawEllipse(p, 32, 6, 24, 5, 0, 0, 0, 0.15f);

        // boots
        p.setColor(0.35f, 0.2f, 0.1f, 1);
        p.fillCircle(20, 8, 7);
        p.fillCircle(44, 8, 7);
        p.fillRectangle(13, 4, 14, 5);
        p.fillRectangle(37, 4, 14, 5);

        // legs
        p.setColor(0.15f, 0.15f, 0.3f, 1);
        p.fillRectangle(15, 13, 12, 14);
        p.fillRectangle(37, 13, 12, 14);

        // tunic/body
        p.setColor(0.18f, 0.28f, 0.48f, 1);
        p.fillCircle(32, 38, 20);

        // belt
        p.setColor(0.4f, 0.25f, 0.1f, 1);
        p.fillRectangle(12, 28, 40, 5);

        // buckle
        p.setColor(0.9f, 0.75f, 0.1f, 1);
        p.fillCircle(32, 30, 4);

        // arms
        p.setColor(0.9f, 0.75f, 0.55f, 1);
        p.fillCircle(8, 34, 7);
        p.fillCircle(56, 34, 7);
        p.fillRectangle(2, 28, 8, 8);
        p.fillRectangle(54, 28, 8, 8);

        // hands
        p.fillCircle(6, 38, 4);
        p.fillCircle(58, 38, 4);

        // head
        p.setColor(0.9f, 0.75f, 0.55f, 1);
        p.fillCircle(32, 48, 10);

        // hair/helmet
        p.setColor(0.4f, 0.4f, 0.5f, 1);
        p.fillCircle(32, 52, 11);
        p.fillRectangle(21, 50, 22, 6);

        // visor
        p.setColor(0.25f, 0.25f, 0.35f, 1);
        p.fillRectangle(24, 49, 16, 3);

        // plume
        p.setColor(0.8f, 0.12f, 0.12f, 1);
        p.fillTriangle(30, 56, 34, 56, 32, 64);
        p.fillCircle(32, 58, 5);

        // sword
        p.setColor(0.7f, 0.7f, 0.8f, 1);
        p.fillRectangle(52, 12, 4, 22);
        p.setColor(0.9f, 0.8f, 0.2f, 1);
        p.fillRectangle(51, 10, 6, 4);
        p.fillRectangle(51, 32, 6, 3);
        p.setColor(0.7f, 0.7f, 0.8f, 1);
        p.fillTriangle(53, 34, 57, 34, 55, 40);

        // shield
        p.setColor(0.2f, 0.2f, 0.5f, 1);
        p.fillCircle(8, 38, 7);
        p.fillRectangle(3, 32, 8, 12);
        p.setColor(0.9f, 0.9f, 0.9f, 1);
        p.fillRectangle(5, 35, 4, 4);

        // outline
        outline(p, 0, 0, 63, 63, 0.15f, 0.1f, 0.05f);

        return make(p);
    }

    public static Texture workerTexture() {
        Pixmap p = new Pixmap(48, 56, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // shadow
        p.setColor(0, 0, 0, 0.15f);
        drawEllipse(p, 24, 6, 18, 4, 0, 0, 0, 0.15f);

        // boots
        p.setColor(0.35f, 0.2f, 0.1f, 1);
        p.fillCircle(14, 8, 6);
        p.fillCircle(34, 8, 6);
        p.fillRectangle(8, 4, 12, 5);
        p.fillRectangle(28, 4, 12, 5);

        // trousers
        p.setColor(0.35f, 0.25f, 0.15f, 1);
        p.fillRectangle(10, 12, 10, 12);
        p.fillRectangle(28, 12, 10, 12);

        // tunic
        p.setColor(0.5f, 0.35f, 0.15f, 1);
        p.fillCircle(24, 30, 16);

        // arms
        p.setColor(0.9f, 0.75f, 0.55f, 1);
        p.fillCircle(6, 28, 6);
        p.fillCircle(42, 28, 6);
        p.fillRectangle(1, 24, 8, 7);
        p.fillRectangle(39, 24, 8, 7);

        // hands
        p.fillCircle(4, 32, 4);
        p.fillCircle(44, 32, 4);

        // head
        p.setColor(0.9f, 0.75f, 0.55f, 1);
        p.fillCircle(24, 40, 8);

        // hat
        p.setColor(0.55f, 0.4f, 0.2f, 1);
        p.fillRectangle(14, 42, 20, 5);
        p.fillRectangle(17, 44, 14, 8);
        p.fillCircle(24, 50, 8);

        // axe
        p.setColor(0.5f, 0.3f, 0.1f, 1);
        p.fillRectangle(40, 12, 3, 16);
        p.setColor(0.6f, 0.6f, 0.6f, 1);
        p.fillTriangle(38, 26, 46, 26, 42, 34);
        p.fillTriangle(39, 26, 45, 26, 42, 20);

        return make(p);
    }

    public static Texture enemyTexture() {
        Pixmap p = new Pixmap(56, 56, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // shadow
        p.setColor(0, 0, 0, 0.15f);
        drawEllipse(p, 28, 6, 22, 4, 0, 0, 0, 0.15f);

        // dark cloak body
        p.setColor(0.3f, 0.08f, 0.08f, 1);
        p.fillCircle(28, 30, 22);

        // hood
        p.setColor(0.35f, 0.1f, 0.1f, 1);
        p.fillCircle(28, 38, 16);

        // face area (dark)
        p.setColor(0.15f, 0.05f, 0.05f, 1);
        p.fillCircle(28, 38, 12);

        // red glowing eyes
        p.setColor(1f, 0.15f, 0.15f, 1);
        p.fillCircle(22, 38, 3);
        p.fillCircle(34, 38, 3);

        // eye glow
        p.setColor(1f, 0.5f, 0.5f, 0.6f);
        p.fillCircle(22, 39, 2);
        p.fillCircle(34, 39, 2);

        // legs
        p.setColor(0.25f, 0.08f, 0.08f, 1);
        p.fillRectangle(14, 6, 10, 14);
        p.fillRectangle(32, 6, 10, 14);

        // boots
        p.setColor(0.15f, 0.05f, 0.05f, 1);
        p.fillCircle(19, 6, 7);
        p.fillCircle(37, 6, 7);
        p.fillRectangle(12, 3, 14, 5);
        p.fillRectangle(30, 3, 14, 5);

        // weapon
        p.setColor(0.4f, 0.4f, 0.45f, 1);
        p.fillRectangle(4, 20, 3, 18);
        p.setColor(0.5f, 0.15f, 0.15f, 1);
        p.fillRectangle(3, 18, 5, 3);

        return make(p);
    }

    public static Texture goldTexture() {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // glow
        p.setColor(1f, 0.9f, 0.2f, 0.15f);
        p.fillCircle(16, 16, 14);

        // coin body
        p.setColor(1f, 0.8f, 0f, 1);
        p.fillCircle(16, 16, 10);

        // highlight
        p.setColor(1f, 0.9f, 0.3f, 1);
        p.fillCircle(14, 14, 6);

        // $ symbol
        p.setColor(0.7f, 0.5f, 0f, 1);
        p.fillRectangle(14, 9, 4, 14);
        p.fillRectangle(11, 11, 10, 3);
        p.fillRectangle(11, 18, 10, 3);

        // shine
        p.setColor(1f, 1f, 1f, 0.5f);
        p.fillCircle(12, 12, 2);

        return make(p);
    }

    public static Texture foodTexture() {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // wheat stalks
        p.setColor(0.8f, 0.7f, 0.1f, 1);
        p.fillRectangle(8, 4, 3, 18);
        p.fillRectangle(14, 6, 3, 16);
        p.fillRectangle(20, 8, 3, 14);

        // wheat heads
        p.fillCircle(10, 18, 4);
        p.fillCircle(17, 20, 4);
        p.fillCircle(22, 20, 4);

        // highlight
        p.setColor(0.95f, 0.85f, 0.2f, 1);
        p.fillCircle(10, 18, 2);

        return make(p);
    }

    public static Texture woodTexture() {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        // log body
        p.setColor(0.4f, 0.25f, 0.1f, 1);
        p.fillRectangle(4, 4, 24, 24);

        // wood grain
        p.setColor(0.5f, 0.35f, 0.15f, 1);
        p.fillRectangle(8, 8, 16, 16);

        // rings
        p.setColor(0.35f, 0.2f, 0.08f, 1);
        p.fillCircle(16, 16, 8);
        p.setColor(0.5f, 0.35f, 0.15f, 1);
        p.fillCircle(16, 16, 5);
        p.setColor(0.35f, 0.2f, 0.08f, 1);
        p.fillCircle(16, 16, 3);

        return make(p);
    }

    public static Texture buildingTexture(String id) {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        float r1, g1, b1, r2, g2, b2, r3, g3, b3;
        switch (id) {
            case "basic_hut":
                r1=0.45f; g1=0.3f; b1=0.15f; r2=0.55f; g2=0.4f; b2=0.2f; r3=0.6f; g3=0.45f; b3=0.25f; break;
            case "woodcutting":
                r1=0.3f; g1=0.2f; b1=0.1f; r2=0.4f; g2=0.3f; b2=0.15f; r3=0.5f; g3=0.35f; b3=0.2f; break;
            case "watchtower":
                r1=0.3f; g1=0.3f; b1=0.35f; r2=0.4f; g2=0.4f; b2=0.45f; r3=0.5f; g3=0.5f; b3=0.55f; break;
            case "monastery":
                r1=0.35f; g1=0.2f; b1=0.2f; r2=0.45f; g2=0.3f; b2=0.3f; r3=0.55f; g3=0.4f; b3=0.4f; break;
            case "pekara":
                r1=0.5f; g1=0.35f; b1=0.15f; r2=0.6f; g2=0.45f; b2=0.2f; r3=0.7f; g3=0.5f; b3=0.25f; break;
            case "kafana":
                r1=0.45f; g1=0.25f; b1=0.1f; r2=0.55f; g2=0.35f; b2=0.15f; r3=0.65f; g3=0.4f; b3=0.2f; break;
            case "kasarna":
                r1=0.25f; g1=0.25f; b1=0.25f; r2=0.35f; g2=0.35f; b2=0.35f; r3=0.45f; g3=0.45f; b3=0.45f; break;
            case "mythic_slot":
                r1=0.3f; g1=0.1f; b1=0.4f; r2=0.4f; g2=0.15f; b2=0.5f; r3=0.5f; g3=0.2f; b3=0.6f; break;
            default:
                r1=0.4f; g1=0.3f; b1=0.2f; r2=0.5f; g2=0.4f; b2=0.3f; r3=0.6f; g3=0.5f; b3=0.4f;
        }

        // wall
        p.setColor(r1, g1, b1, 1);
        p.fillRectangle(2, 2, 28, 18);

        // wall highlight
        p.setColor(r3, g3, b3, 1);
        p.fillRectangle(2, 16, 28, 4);

        // roof
        p.setColor(r2, g2, b2, 1);
        p.fillTriangle(0, 20, 32, 20, 16, 32);

        // roof highlight
        p.setColor(r3, g3, b3, 0.5f);
        p.fillTriangle(4, 22, 28, 22, 16, 30);

        // door
        p.setColor(0.2f, 0.15f, 0.1f, 1);
        p.fillRectangle(12, 2, 8, 12);

        // door arch
        p.setColor(0.25f, 0.18f, 0.12f, 1);
        p.fillRectangle(13, 2, 6, 11);

        // windows
        p.setColor(0.9f, 0.9f, 0.6f, 1);
        p.fillRectangle(4, 8, 6, 5);
        p.fillRectangle(22, 8, 6, 5);

        // window glow
        p.setColor(1f, 0.95f, 0.7f, 0.5f);
        p.fillRectangle(5, 9, 4, 3);
        p.fillRectangle(23, 9, 4, 3);

        return make(p);
    }

    public static Texture buildingSlotTexture() {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        p.setColor(0.6f, 0.5f, 0.3f, 0.4f);
        p.fillRectangle(1, 1, 30, 30);

        p.setColor(0.8f, 0.7f, 0.4f, 0.6f);
        p.drawRectangle(2, 2, 28, 28);
        p.drawRectangle(4, 4, 24, 24);

        p.setColor(0.9f, 0.8f, 0.5f, 0.4f);
        p.fillRectangle(6, 6, 2, 2);
        p.fillRectangle(24, 6, 2, 2);
        p.fillRectangle(6, 24, 2, 2);
        p.fillRectangle(24, 24, 2, 2);

        return make(p);
    }

    public static Texture terrainTexture(String type) {
        Pixmap p = new Pixmap(32, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        switch (type) {
            case "grass":
                p.setColor(0.18f, 0.32f, 0.12f, 1);
                p.fill();
                p.setColor(0.22f, 0.36f, 0.15f, 1);
                p.fillCircle(6, 4, 4);
                p.fillCircle(22, 22, 5);
                p.fillCircle(10, 26, 3);
                break;
            case "dirt":
                p.setColor(0.42f, 0.28f, 0.1f, 1);
                p.fill();
                p.setColor(0.38f, 0.24f, 0.08f, 1);
                p.fillCircle(8, 6, 5);
                p.fillCircle(22, 20, 5);
                break;
            case "water":
                p.setColor(0.15f, 0.28f, 0.58f, 1);
                p.fill();
                p.setColor(0.2f, 0.35f, 0.65f, 0.5f);
                p.fillCircle(8, 8, 6);
                p.fillCircle(22, 18, 5);
                p.fillCircle(10, 24, 4);
                break;
            case "forest":
                p.setColor(0.06f, 0.2f, 0.06f, 1);
                p.fill();
                p.setColor(0.1f, 0.28f, 0.08f, 1);
                p.fillCircle(16, 16, 14);
                p.setColor(0.08f, 0.22f, 0.06f, 1);
                p.fillCircle(10, 22, 10);
                p.fillCircle(24, 20, 8);
                break;
            case "stone":
                p.setColor(0.35f, 0.35f, 0.35f, 1);
                p.fill();
                p.setColor(0.4f, 0.4f, 0.4f, 1);
                p.fillCircle(8, 6, 6);
                p.fillCircle(22, 22, 5);
                p.setColor(0.3f, 0.3f, 0.3f, 1);
                p.fillCircle(10, 22, 4);
                break;
        }

        return make(p);
    }

    public static Texture terrainIsoTexture(String type) {
        Pixmap p = new Pixmap(64, 32, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        switch (type) {
            case "grass": {
                fillDiamond(p, 0.2f, 0.38f, 0.14f, 1);
                fillDiamond(p, 0.25f, 0.42f, 0.17f, 0.6f);
                p.setColor(0.3f, 0.48f, 0.2f, 0.4f);
                p.fillCircle(24, 12, 5);
                p.fillCircle(44, 18, 4);
                break;
            }
            case "dirt": {
                fillDiamond(p, 0.45f, 0.32f, 0.12f, 1);
                p.setColor(0.38f, 0.26f, 0.1f, 0.5f);
                p.fillCircle(20, 10, 5);
                p.fillCircle(46, 22, 4);
                p.setColor(0.5f, 0.38f, 0.16f, 0.3f);
                p.fillCircle(32, 16, 6);
                break;
            }
            case "water": {
                fillDiamond(p, 0.12f, 0.25f, 0.55f, 1);
                p.setColor(0.2f, 0.35f, 0.65f, 0.4f);
                p.fillCircle(22, 10, 6);
                p.fillCircle(44, 20, 5);
                p.setColor(0.3f, 0.5f, 0.8f, 0.3f);
                p.fillCircle(32, 16, 8);
                break;
            }
            case "forest": {
                fillDiamond(p, 0.08f, 0.22f, 0.06f, 1);
                p.setColor(0.12f, 0.32f, 0.08f, 0.7f);
                p.fillCircle(24, 14, 8);
                p.fillCircle(42, 18, 6);
                p.setColor(0.06f, 0.18f, 0.04f, 0.5f);
                p.fillCircle(16, 10, 5);
                p.fillCircle(50, 22, 5);
                break;
            }
            case "stone": {
                fillDiamond(p, 0.38f, 0.38f, 0.38f, 1);
                p.setColor(0.45f, 0.45f, 0.45f, 0.5f);
                p.fillCircle(20, 10, 6);
                p.fillCircle(46, 22, 5);
                p.setColor(0.32f, 0.32f, 0.32f, 0.4f);
                p.fillCircle(32, 16, 7);
                break;
            }
        }

        return make(p);
    }

    private static void fillDiamondCentered(Pixmap p, int cx, int cy, int halfW, int halfH, float r, float g, float b, float a) {
        p.setColor(r, g, b, a);
        p.fillTriangle(cx, cy, cx + halfW, cy, cx, cy - halfH);
        p.fillTriangle(cx, cy, cx + halfW, cy, cx, cy + halfH);
        p.fillTriangle(cx, cy, cx - halfW, cy, cx, cy + halfH);
        p.fillTriangle(cx, cy, cx - halfW, cy, cx, cy - halfH);
    }

    public static Texture buildingIsoTexture(String id) {
        Pixmap p = new Pixmap(64, 56, Pixmap.Format.RGBA8888);
        p.setColor(0, 0, 0, 0);
        p.fill();

        float r1, g1, b1, r2, g2, b2, r3, g3, b3;
        switch (id) {
            case "basic_hut":
                r1=0.45f; g1=0.3f; b1=0.15f; r2=0.55f; g2=0.4f; b2=0.2f; r3=0.6f; g3=0.45f; b3=0.25f; break;
            case "woodcutting":
                r1=0.3f; g1=0.2f; b1=0.1f; r2=0.4f; g2=0.3f; b2=0.15f; r3=0.5f; g3=0.35f; b3=0.2f; break;
            case "watchtower":
                r1=0.3f; g1=0.3f; b1=0.35f; r2=0.4f; g2=0.4f; b2=0.45f; r3=0.5f; g3=0.5f; b3=0.55f; break;
            case "monastery":
                r1=0.35f; g1=0.2f; b1=0.2f; r2=0.45f; g2=0.3f; b2=0.3f; r3=0.55f; g3=0.4f; b3=0.4f; break;
            case "pekara":
                r1=0.5f; g1=0.35f; b1=0.15f; r2=0.6f; g2=0.45f; b2=0.2f; r3=0.7f; g3=0.5f; b3=0.25f; break;
            case "kafana":
                r1=0.45f; g1=0.25f; b1=0.1f; r2=0.55f; g2=0.35f; b2=0.15f; r3=0.65f; g3=0.4f; b3=0.2f; break;
            case "kasarna":
                r1=0.25f; g1=0.25f; b1=0.25f; r2=0.35f; g2=0.35f; b2=0.35f; r3=0.45f; g3=0.45f; b3=0.45f; break;
            case "mythic_slot":
                r1=0.3f; g1=0.1f; b1=0.4f; r2=0.4f; g2=0.15f; b2=0.5f; r3=0.5f; g3=0.2f; b3=0.6f; break;
            default:
                r1=0.4f; g1=0.3f; b1=0.2f; r2=0.5f; g2=0.4f; b2=0.3f; r3=0.6f; g3=0.5f; b3=0.4f;
        }

        // shadow
        drawEllipse(p, 32, 48, 24, 5, 0, 0, 0, 0.12f);

        // left wall face (parallelogram as two triangles)
        p.setColor(r1, g1, b1, 1);
        p.fillTriangle(8, 44, 32, 44, 8, 24);
        p.fillTriangle(32, 44, 32, 24, 8, 24);

        // right wall face
        p.fillTriangle(32, 44, 56, 44, 32, 24);
        p.fillTriangle(56, 44, 56, 24, 32, 24);

        // wall highlights
        p.setColor(r3, g3, b3, 0.3f);
        p.fillTriangle(32, 44, 32, 24, 8, 24);
        p.fillTriangle(32, 44, 56, 44, 32, 24);

        // roof diamond
        fillDiamondCentered(p, 32, 20, 24, 12, r2, g2, b2, 1);

        // roof highlight
        p.setColor(r3, g3, b3, 0.4f);
        p.fillTriangle(32, 8, 48, 18, 32, 18);
        p.fillTriangle(32, 8, 16, 18, 32, 18);

        // door on right wall face
        p.setColor(0.2f, 0.15f, 0.1f, 1);
        p.fillRectangle(28, 30, 8, 14);

        // door arch
        p.setColor(0.25f, 0.18f, 0.12f, 1);
        p.fillRectangle(29, 32, 6, 12);

        // window on left wall face
        p.setColor(0.9f, 0.9f, 0.6f, 1);
        p.fillRectangle(12, 32, 7, 7);

        // window glow
        p.setColor(1f, 0.95f, 0.7f, 0.4f);
        p.fillRectangle(13, 33, 5, 5);

        return make(p);
    }
}
