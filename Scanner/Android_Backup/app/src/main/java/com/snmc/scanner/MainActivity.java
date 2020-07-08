package com.snmc.scanner;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import android.Manifest;
import android.content.pm.PackageManager;
import android.media.MediaPlayer;
import android.os.Bundle;
import android.os.Handler;
import android.os.StrictMode;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.budiyev.android.codescanner.CodeScanner;
import com.budiyev.android.codescanner.CodeScannerView;
import com.budiyev.android.codescanner.DecodeCallback;
import com.google.zxing.Result;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

public class MainActivity extends AppCompatActivity {
    CodeScanner codeScanner;
    Button continueButton;
    CodeScannerView scannerView;
    String guid;
    static final int PERMISSIONS_CODE = 1;
    static final int BUTTON_TIMEOUT = 2000;
    MediaPlayer successNotification;
    MediaPlayer unverifiedNotification;
    MediaPlayer failureNotification;

    HttpURLConnection conn;
    BufferedReader bufferedReader;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
        StrictMode.setThreadPolicy(policy);
        setContentView(R.layout.activity_main);

        // Check Permissions and Request if Necessary
        checkPermissions();

        successNotification = MediaPlayer.create(this, R.raw.successnotification);
        unverifiedNotification = MediaPlayer.create(this, R.raw.unverifiednotification);
        failureNotification = MediaPlayer.create(this, R.raw.failurenotification);

        continueButton = findViewById(R.id.continueButton);
        scannerView = findViewById(R.id.scannerView);
        codeScanner = new CodeScanner(this, scannerView);

        codeScanner.setDecodeCallback(new DecodeCallback() {
            @Override
            public void onDecoded(@NonNull final Result result) {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        guid = result.getText();
                        Toast.makeText(MainActivity.this, guid, Toast.LENGTH_SHORT).show();
                        makePost(guid);
                    }
                });
            }
        });

        continueButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                startPreview();
            }
        });
    }

    private void checkPermissions() {
        if (
                ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED
        ) {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.CAMERA}, PERMISSIONS_CODE);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == PERMISSIONS_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                startPreview();
            } else {
                closeApp();
            }
        }
    }

    private void closeApp() {
        finish();
        System.exit(0);
    }

    private void makePost(String guid) {
        try {
            JSONObject json = new JSONObject();
            json.put("VisitorId", guid);
            json.put("Organization","SNMC");
            json.put("Door", "Main");
            json.put("Direction", "In");

            String body = json.toString();

            String url = "https://api.track.mysnmc.ca/api/visits";
            URL urlObj = new URL(url);
            conn = (HttpURLConnection) urlObj.openConnection();
            conn.setRequestMethod("POST");
            conn.setRequestProperty("x-functions-key", "uxOYAbny7F/aRJmCs4ItP31JKY2bf5c4KppYADJY/HOtiwoeX4qjJw==");
            conn.setDoInput(true);
            conn.setDoOutput(true);

            OutputStream os = conn.getOutputStream();
            byte[] input = body.getBytes(StandardCharsets.UTF_8);
            os.write(input, 0, input.length);

            InputStreamReader inputStreamReader;

            conn.connect();

            if (conn.getResponseCode() == 200) {
                inputStreamReader = new InputStreamReader(conn.getInputStream(), StandardCharsets.UTF_8);
            } else {
                inputStreamReader = new InputStreamReader(conn.getErrorStream(), StandardCharsets.UTF_8);
            }

            bufferedReader = new BufferedReader(
                    inputStreamReader);
            StringBuilder response = new StringBuilder();
            String responseLine;
            while ((responseLine = bufferedReader.readLine()) != null) {
                response.append(responseLine.trim());
            }

            Log.v("Scanner",Integer.toString(conn.getResponseCode()));
            Log.v("Scanner",conn.getResponseMessage());
            Log.v("Scanner",response.toString());


            Toast.makeText(MainActivity.this, response.toString(), Toast.LENGTH_SHORT).show();
            if (conn.getResponseCode() == 200) {
                successIndicator();
            } else if (conn.getResponseCode() == 402) {
                unverifiedIndicator();
            } else {
                failureIndicator();
            }
        } catch (IOException | JSONException e) {
            e.printStackTrace();
            Log.v("Response Failure", "Exception Occurred");
            failureIndicator();
        } finally {
            if (bufferedReader != null) {
                try {
                    bufferedReader.close();
                } catch (IOException e) {
                    e.printStackTrace();
                    Log.v("Response Failure", "Exception Occurred");
                    failureIndicator();
                }
            }
            if (conn != null) {
                conn.disconnect();
            }
        }
    }

    private void successIndicator() {
        continueButton.setText(getResources().getText(R.string.success_indicator));
        continueButton.setBackgroundColor(getResources().getColor(R.color.successIndicator));
        continueButton.setVisibility(View.VISIBLE);
        continueButton.setEnabled(true);
        successNotification.start();
        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                startPreview();
            }
        }, BUTTON_TIMEOUT);
    }

    private void unverifiedIndicator() {
        continueButton.setText(getResources().getText(R.string.unverified_indicator));
        continueButton.setVisibility(View.VISIBLE);
        continueButton.setBackgroundColor(getResources().getColor(R.color.unverifiedIndicator));
        continueButton.setEnabled(true);
        unverifiedNotification.start();
        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                startPreview();
            }
        }, BUTTON_TIMEOUT);
    }

    private void failureIndicator() {
        continueButton.setText(getResources().getText(R.string.failure_indicator));
        continueButton.setVisibility(View.VISIBLE);
        continueButton.setBackgroundColor(getResources().getColor(R.color.failureIndicator));
        continueButton.setEnabled(true);
        failureNotification.start();
        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                startPreview();
            }
        }, BUTTON_TIMEOUT);
    }

    private void hideButton() {
        continueButton.setEnabled(false);
        continueButton.setText(getResources().getText(R.string.disabled_indicator));
        continueButton.setBackgroundColor(getResources().getColor(R.color.disabledIndicator));
        continueButton.setVisibility(View.GONE);
    }

    private void startPreview() {
        hideButton();
        codeScanner.startPreview();
    }

    @Override
    protected void onResume() {
        super.onResume();
        checkPermissions();
        startPreview();
    }
}