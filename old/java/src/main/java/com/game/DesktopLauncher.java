package com.game;

import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3ApplicationConfiguration;

public class DesktopLauncher {
    public static void main(String[] args) {
        org.lwjgl.system.Configuration.GLFW_CHECK_THREAD0.set(false);

        Lwjgl3ApplicationConfiguration config = new Lwjgl3ApplicationConfiguration();
        config.setTitle("From Zero to Hero \u2014 City Builder Survival");
        config.setWindowedMode(1280, 720);
        config.setForegroundFPS(60);
        config.setResizable(true);
        new Lwjgl3Application(new FromZeroToHero(), config);
    }
}
