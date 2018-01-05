#! /usr/bin/expect
if {$argc > 0} {
  set ip [lindex $argv 0]
  if { $ip < 10 } {
  #  spawn ssh zelin@222.200.182.82
    spawn ssh zelin@222.200.182.37
    expect "*password*"
    send "zelin\r"
    expect "*sagittarius*"
    # spawn ssh "192.168.1.$ip"
    send "ssh zelin@192.168.1.$ip\r"
    expect {
      "*yes/no*" {send "yes\r"; exp_continue}
      "*password*" {send "zelin\r"}
    }
    interact
  }
  if { $ip > 10 } {
    spawn ssh zelin@222.200.182.82
   # spawn ssh zelin@222.200.182.37
    expect "*password*"
    send "zelin\r"
    expect "*sagittarius*"
    # spawn ssh "192.168.1.$ip"
    send "ssh zelin@192.168.1.$ip\r"
    expect {
      "*yes/no*" {send "yes\r"; exp_continue}
      "*password*" {send "zelin\r"}
    }
    interact
  }
}
if {$argc < 1} {
  spawn ssh zelin@222.200.182.82
  expect "*password*"
  send "zelin\r"
  interact
}

