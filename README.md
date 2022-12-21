# MailSender
Mail password can be obtained by enabling a 2 Step Verification on your google account that you can find by navigating to:
Google Account -> Security -> 2 Step Verification
and then adding a new App Password(below 2SV)

You can select a text file with all the Senders by clicking the Select Senders button.
Senders text file should be written in a following format:

mail1@gmail.com password

mail2@gmail.com password

mail3@gmail.com password

.

.

.


You can select a text file with all the Receivers by clicking the Select Receivers button.

Senders text file should be written in a following format:

mail1@gmail.com

mail2@gmail.com

mail3@gmail.com

.

.

.


Subject is the Subject of all the mails being sent.

Body is the Body of all the mails being sent.

Select Body allows you to load a body from a text file.

Repeat determines the amount of times each sender will send a mail to each receiver(I'd advise you against using anything above 100).

Interval repeats the sending process after given amount of seconds if Use Interval is enabled.

Pause makes the program wait for given amount of seconds if sending a mail fails(use this if you are sending 90+ mails in total and set it to 90+, else just set it to 0).

Is HTML determines if the body of the mail is written in HTML.

Start starts the sending process.

Reset resets the Sent and Failed label values.

DISCLAIMER: THIS PROJECT IS FOR ACADEMIC PURPOSES ONLY. I TAKE NO RESPONSIBILITY FOR ILLEGAL USAGE AND/OR POTENTIAL HARMS.
