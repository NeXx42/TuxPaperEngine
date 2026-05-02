#!/home/matth/steamcmd-venv/bin/python3
import sys
import pexpect

PROMPTS = [
    r'Steam>',
    r'password:',
    r'Two-factor code:',
    r'Steam Guard code:',
    r'Please confirm the login in the Steam Mobile app on your phone.',
    pexpect.EOF,
    pexpect.TIMEOUT
]

child = pexpect.spawn('steamcmd', encoding='utf-8', timeout=120)

while True:
    index = child.expect(PROMPTS, timeout=120)

    print(child.before or '', end='', flush=True)
    print('\x1E', flush=True)

    if index >= len(PROMPTS) - 2:
        break

    line = sys.stdin.readline()
    if not line:
        break
    child.sendline(line.strip())