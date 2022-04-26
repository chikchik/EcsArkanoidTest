import com.github.jengelman.gradle.plugins.shadow.tasks.ShadowJar

buildscript {
    repositories {
        jcenter()
        mavenCentral()
    }

    dependencies {
        classpath(kotlin("gradle-plugin", version = "1.3.72"))
    }
}

val coroutinesVersion = "1.3.7"
val ktor_version = "1.3.2"
val kotlin_version = "1.3.70"

plugins {
    application
    idea
    kotlin("jvm") version "1.3.72"
    id("com.github.johnrengelman.shadow") version "4.0.4"
}

repositories {
    mavenLocal()
    google()
    jcenter()
    mavenCentral()
}

dependencies {

    attributesSchema {
        attribute(org.jetbrains.kotlin.gradle.plugin.KotlinPlatformType.attribute)
    }

    implementation(kotlin("stdlib"))
    implementation("javax.annotation:javax.annotation-api:1.2")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:$coroutinesVersion")


    implementation("org.jetbrains.kotlin:kotlin-stdlib-jdk8:$kotlin_version")
    implementation("io.ktor:ktor-server-netty:$ktor_version")
    implementation("io.ktor:ktor-server-core:$ktor_version")
    implementation("io.ktor:ktor-html-builder:$ktor_version")
    implementation("io.ktor:ktor-server-host-common:$ktor_version")
    implementation("io.ktor:ktor-jackson:$ktor_version")
    implementation("io.ktor:ktor-mustache:$ktor_version")
    implementation("org.kodein.di:kodein-di:7.0.0")
    implementation("io.ktor:ktor-websockets:$ktor_version")
    testImplementation ("io.ktor:ktor-server-tests:$ktor_version")
    implementation("io.ktor:ktor-client-logging-jvm:$ktor_version")
    testImplementation("io.ktor:ktor-client-logging-jvm:$ktor_version")
}


tasks {
    named<ShadowJar>("shadowJar") {
        archiveBaseName.set("shadow")
        mergeServiceFiles()
        manifest {
            attributes(mapOf("Main-Class" to "com.dm.MainKt"))
        }
    }
}


configurations.all {
    afterEvaluate {
        if (isCanBeResolved) {
            attributes {
                attribute(org.jetbrains.kotlin.gradle.plugin.KotlinPlatformType.attribute, org.jetbrains.kotlin.gradle.plugin.KotlinPlatformType.jvm)
            }
        }
    }
}


java {
    sourceCompatibility = JavaVersion.VERSION_1_8
}

application {
    mainClassName = "com.dm.MainKt"
}

kotlin {
    experimental {
        coroutines = org.jetbrains.kotlin.gradle.dsl.Coroutines.ENABLE
    }
}

tasks.withType<org.jetbrains.kotlin.gradle.tasks.KotlinCompile> {
    kotlinOptions {
        jvmTarget = "1.8"
        freeCompilerArgs = listOf("-Xuse-experimental=kotlin.Experimental")
    }
}
