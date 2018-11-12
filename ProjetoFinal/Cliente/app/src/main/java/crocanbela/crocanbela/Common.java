package crocanbela.crocanbela;

import android.os.Handler;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

/**
 * Created by perninha on 12/11/18.
 */

public class Common {

    private Handler handler = new Handler();

    public void AtualizarProgress(final String status, final boolean erro, final boolean finalize
            , final ProgressBar progress, final TextView txtProgress){

        new Thread(new Runnable() {
            @Override
            public void run() {
                handler.post(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            if (erro) {
                                progress.setVisibility(View.INVISIBLE);
                                txtProgress.setText(status);
                            } else {
                                txtProgress.setText(status);

                                if(finalize){
                                    progress.setVisibility(View.INVISIBLE);
                                }
                            }
                        }catch (Exception e){
                            txtProgress.setText(e.getMessage());
                        }
                    }
                });
            }
        }).start();
    }
}
